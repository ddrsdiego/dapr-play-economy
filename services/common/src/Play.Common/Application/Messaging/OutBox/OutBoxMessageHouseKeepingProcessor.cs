namespace Play.Common.Application.Messaging.OutBox;

using System.Threading;
using System.Threading.Tasks;

public sealed class OutBoxMessageHouseKeepingProcessor : BoxMessageHouseKeepingProcessor
{
    private Task _processingTask;
    private readonly IOutBoxMessageHouseKeepingRepository _outBoxMessageHouseKeepingRepository;

    public OutBoxMessageHouseKeepingProcessor(IOutBoxMessageHouseKeepingRepository outBoxMessageHouseKeepingRepository)
    {
        _outBoxMessageHouseKeepingRepository = outBoxMessageHouseKeepingRepository;
    }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        _processingTask = Task.Run(async () => await RunAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    private Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
        }
        
        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        await _processingTask;
        _processingTask = null;
    }
}