namespace Play.Common.Application.Infra.Outbox;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Messaging;

internal static class OutBoxMessageEx
{
    public static MessageEnvelope ToEnvelopeMessage(this OutBoxMessage outBoxMessage)
    {
        return new MessageEnvelope(outBoxMessage.EventName,
            outBoxMessage.TopicName,
            "",
            Encoding.UTF8.GetBytes(outBoxMessage.Payload));
    }
}

public interface IOutboxMessagesProcessor
{
    Task RunAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}

public class OutboxMessagesProcessorConfig
{
    public string PubSubName { get; set; }
    public TimeSpan ProcessingIntervalInSeconds { get; set; }
    public int MaxProcessingMessagesCount { get; set; }
}

public sealed class OutboxMessagesProcessor : IOutboxMessagesProcessor
{
    private Task _processingTask;
    private readonly DaprClient _daprClient;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly OutboxMessagesProcessorConfig _config;
    private readonly IOutboxMessagesRepository _outboxMessagesRepository;

    public OutboxMessagesProcessor(OutboxMessagesProcessorConfig config,
        IOutboxMessagesRepository outboxMessagesRepository,
        IUnitOfWorkFactory unitOfWorkFactory, DaprClient daprClient)
    {
        _config = config;
        _outboxMessagesRepository = outboxMessagesRepository;
        _unitOfWorkFactory = unitOfWorkFactory;
        _daprClient = daprClient;
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        _processingTask = Task.Run(() => ProcessMessages(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _processingTask.Dispose();
        await Task.CompletedTask;
    }

    private async Task ProcessMessages(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messagesPendingToPublish =
                await _outboxMessagesRepository.GetMessagesPendingToPublishAsync(cancellationToken);
            if (messagesPendingToPublish.Length > 0)
            {
                for (var index = 0; index < messagesPendingToPublish.Length; index++)
                {
                    var outBoxMessage = messagesPendingToPublish[index];

                    try
                    {
                        await using var uow = await _unitOfWorkFactory.CreateAsync(cancellationToken);

                        uow.AddToContext(async () => await _daprClient.PublishEventAsync(_config.PubSubName, outBoxMessage.TopicName, outBoxMessage.ToEnvelopeMessage(), cancellationToken));
                        uow.AddToContext(async () => await _outboxMessagesRepository.UpdateToPublishedAsync(outBoxMessage.ToMessagePublished(), cancellationToken));

                        await uow.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        await _outboxMessagesRepository.IncrementNumberAttemptsAsync(outBoxMessage.ToMessagePublished(),
                            e.ToString(), cancellationToken);
                    }
                }
            }

            await Task.Delay(_config.ProcessingIntervalInSeconds, cancellationToken);
        }
    }
}