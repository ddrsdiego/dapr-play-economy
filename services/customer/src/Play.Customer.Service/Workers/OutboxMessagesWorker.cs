namespace Play.Customer.Service.Workers;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Infra;
using Common.Application.Infra.Outbox;
using Common.Messaging;
using Dapr.Client;
using Microsoft.Extensions.Hosting;

internal sealed class OutboxMessagesWorker : BackgroundService
{
    private const string PubSubName = "play-customer-service-pubsub";

    private readonly DaprClient _daprClient;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IOutboxMessagesRepository _outboxMessagesRepository;

    public OutboxMessagesWorker(IUnitOfWorkFactory unitOfWorkFactory,
        IOutboxMessagesRepository outboxMessagesRepository,
        DaprClient daprClient)
    {
        _daprClient = daprClient;
        _unitOfWorkFactory = unitOfWorkFactory;
        _outboxMessagesRepository = outboxMessagesRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messagesPendingToPublish =
                await _outboxMessagesRepository.GetMessagesPendingToPublishAsync(stoppingToken);
            if (messagesPendingToPublish.Length > 0)
            {
                for (var index = 0; index < messagesPendingToPublish.Length; index++)
                {
                    var outBoxMessage = messagesPendingToPublish[index];
                    try
                    {
                        await using var uow = await _unitOfWorkFactory.CreateAsync(stoppingToken);
                        
                        await _daprClient.PublishEventAsync(PubSubName, outBoxMessage.TopicName, outBoxMessage.ToMessageEnvelope(), stoppingToken);
                        await _outboxMessagesRepository.UpdateToPublishedAsync(outBoxMessage.ToMessagePublished(), stoppingToken);
                        
                        await uow.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception e)
                    {
                        await _outboxMessagesRepository.IncrementNumberAttemptsAsync(outBoxMessage.ToMessagePublished(), stoppingToken);
                        Console.WriteLine(e);
                    }
                }
            }

            await Task.Delay(5_000, stoppingToken);
        }
    }
}

internal static class OutBoxMessageEx
{
    public static MessageEnvelope ToMessageEnvelope(this OutBoxMessage outBoxMessage)
    {
        return new MessageEnvelope(outBoxMessage.Payload, Encoding.UTF8.GetBytes(outBoxMessage.Payload));
    }
}