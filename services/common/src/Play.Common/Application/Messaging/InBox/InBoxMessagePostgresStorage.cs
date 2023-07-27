namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public interface IInBoxMessageStorage :
    IInBoxMessageStorageRead,
    IInBoxMessageStorageWrite
{
}

internal sealed class InBoxMessagePostgresStorage : IInBoxMessageStorage
{
    private readonly IInBoxMessageStorageRead _inBoxMessageStorageRead;
    private readonly IInBoxMessageStorageWrite _inBoxMessageStorageWrite;

    public InBoxMessagePostgresStorage(IInBoxMessageStorageRead inBoxMessageStorageRead, IInBoxMessageStorageWrite inBoxMessageStorageWrite)
    {
        _inBoxMessageStorageRead = inBoxMessageStorageRead;
        _inBoxMessageStorageWrite = inBoxMessageStorageWrite;
    }

    public IAsyncEnumerable<InBoxMessage> GetPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        return _inBoxMessageStorageRead.GetPendingMessagesAsync(cancellationToken);
    }

    public async IAsyncEnumerable<InBoxMessage> GetPendingMessagesByPriorityAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var inBoxMessage in _inBoxMessageStorageRead.GetPendingMessagesByPriorityAsync(cancellationToken))
        {
            yield return inBoxMessage;
        }
    }
    
    public Task IncrementRetryNumberAttemptsAsync(InBoxMessage inBoxMessage, string errorMessage, CancellationToken cancellationToken = default)
    {
        return _inBoxMessageStorageWrite.IncrementRetryNumberAttemptsAsync(inBoxMessage, errorMessage, cancellationToken);
    }

    public Task CommitMessageAsync(InBoxMessage inBoxMessage, CancellationToken cancellationToken = default)
    {
        return _inBoxMessageStorageWrite.CommitMessageAsync(inBoxMessage, cancellationToken);
    }

    public Task SaveAsync(ArraySegment<InBoxMessage> inBoxMessages, CancellationToken cancellationToken = default)
    {
        return _inBoxMessageStorageWrite.SaveAsync(inBoxMessages, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _inBoxMessageStorageRead.DisposeAsync();
    }
}