namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Infra.UoW;

public interface IInBoxMessagesProcessor
{
    IAsyncEnumerable<InBoxMessage> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default);

    Task MarkMessageAsFailedAsync(InBoxMessage message, CancellationToken cancellationToken);

    Task MarkMessageAsProcessedAsync(InBoxMessage message, CancellationToken cancellationToken);
}

public sealed class InBoxMessagesProcessor : BoxMessagesProcessor, IInBoxMessagesProcessor
{
    private Task _processingTask;
    private readonly IInBoxMessagesRepository _inBoxMessagesRepository;

    public InBoxMessagesProcessor(BoxMessagesProcessorConfig config, IInBoxMessagesRepository inBoxMessagesRepository, IUnitOfWorkFactory unitOfWorkFactory, DaprClient daprClient)
        : base(config, unitOfWorkFactory, daprClient)
    {
        _inBoxMessagesRepository = inBoxMessagesRepository;
    }

    public async IAsyncEnumerable<InBoxMessage> GetUnprocessedMessagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const string resourceId = "inbox-messages-lockstore";

        while (!cancellationToken.IsCancellationRequested)
        {
            await using var lockResponse = await DaprClient.Lock(Config.LockStoreName, resourceId, LockOwner, Config.ExpiryInSeconds, cancellationToken: cancellationToken);
            if (!lockResponse.Success)
                continue;

            var filter = new InBoxMessagesRepositoryFilter
            {
                BatchSize = Config.MaxProcessingMessagesCount,
                MaxNumberAttempts = Config.MaxNumberAttempts
            };

            var inBoxMessages = await _inBoxMessagesRepository.GetUnprocessedMessagesAsync(filter, cancellationToken);
            if (inBoxMessages.Length > 0)
            {
                for (var index = 0; index < inBoxMessages.Length; index++)
                {
                    yield return inBoxMessages[index];
                }
            }

            await DaprClient.Unlock(Config.LockStoreName, resourceId, LockOwner, cancellationToken: cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(Config.ProcessingIntervalInSeconds), cancellationToken);
        }
    }

    public Task MarkMessageAsProcessedAsync(InBoxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            return _inBoxMessagesRepository.MarkMessageAsProcessedAsync(message, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task MarkMessageAsFailedAsync(InBoxMessage message, CancellationToken cancellationToken)
    {
        return _inBoxMessagesRepository.MarkMessageAsFailedAsync(message, cancellationToken);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _processingTask = Task.Run(() => ProcessMessages(cancellationToken), cancellationToken);
        await _processingTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    private async Task ProcessMessages(CancellationToken cancellationToken = default)
    {
        const string resourceId = "inbox-messages-lockstore";

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var lockResponse = await DaprClient.Lock(Config.LockStoreName, resourceId, LockOwner, Config.ExpiryInSeconds, cancellationToken: cancellationToken);
                if (!lockResponse.Success)
                    continue;
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