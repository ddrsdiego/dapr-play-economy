namespace Play.Common.Application.Messaging.OutBox;

using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Extensions;
using Infra.UoW;

public interface IOutBoxMessagesProcessor
{
    Task RunAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}

public sealed class OutBoxMessagesProcessor : BoxMessagesProcessor, IOutBoxMessagesProcessor
{
    private Task _processingTask;
    private readonly IOutBoxMessagesRepository _outBoxMessagesRepository;

    public OutBoxMessagesProcessor(BoxMessagesProcessorConfig config, IOutBoxMessagesRepository outBoxMessagesRepository, IUnitOfWorkFactory unitOfWorkFactory, DaprClient daprClient)
        : base(config, unitOfWorkFactory, daprClient)
    {
        _outBoxMessagesRepository = outBoxMessagesRepository;
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        _processingTask = Task.Run(() => ProcessMessages(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _processingTask;
        _processingTask.Dispose();
    }

    private async Task ProcessMessages(CancellationToken cancellationToken = default)
    {
        const string resourceId = "outbox-messages-lockstore";

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var lockResponse = await DaprClient.Lock(Config.LockStoreName, resourceId, LockOwner, Config.ExpiryInSeconds, cancellationToken: cancellationToken);
                if (!lockResponse.Success)
                    continue;

                var filter = new OutBoxMessagesRepositoryFilter
                {
                    NumberAttempts = Config.MaxNumberAttempts,
                    BatchSize = Config.MaxProcessingMessagesCount
                };

                await foreach (var outBoxMessage in _outBoxMessagesRepository.FetchUnprocessedAsync(filter, cancellationToken))
                {
                    try
                    {
                        await using var uow = await UnitOfWorkFactory.CreateAsync(cancellationToken);

                        uow.AddToContext(async () => await DaprClient.PublishEventAsync(Config.PubSubName, outBoxMessage.TopicName, outBoxMessage.ToEnvelopeMessage(), cancellationToken));
                        uow.AddToContext(async () => await _outBoxMessagesRepository.UpdateToPublishedAsync(outBoxMessage, cancellationToken));

                        await uow.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        await using var uow = await UnitOfWorkFactory.CreateAsync(cancellationToken);

                        if (outBoxMessage.NumberAttempts <= Config.MaxNumberAttempts)
                            uow.AddToContext(async () => await _outBoxMessagesRepository.IncrementNumberAttemptsAsync(outBoxMessage, e.ToString(), cancellationToken));
                        else
                            uow.AddToContext(async () => await _outBoxMessagesRepository.IncrementNumberAttemptsAsync(outBoxMessage, e.ToString(), cancellationToken));

                        await uow.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                await DaprClient.Unlock(Config.LockStoreName, resourceId, LockOwner, cancellationToken: cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(Config.ProcessingIntervalInSeconds), cancellationToken);
        }
    }
}