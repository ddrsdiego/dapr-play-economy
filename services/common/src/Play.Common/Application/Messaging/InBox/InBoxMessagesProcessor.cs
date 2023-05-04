namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Infra.UoW;

public interface IInBoxMessagesProcessor
{
    Task RunAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
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