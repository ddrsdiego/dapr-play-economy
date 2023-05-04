namespace Play.Common.Application.Messaging;

using System;
using System.Threading;
using System.Threading.Tasks;

public abstract class BoxMessageHouseKeepingProcessor : IAsyncDisposable
{
    public abstract Task StartAsync(CancellationToken cancellationToken = default);
    
    public abstract ValueTask DisposeAsync();
}