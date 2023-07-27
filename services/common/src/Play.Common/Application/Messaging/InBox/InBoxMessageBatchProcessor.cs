namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IInBoxMessageBatchProcessor
{
    Task ProcessBatchAsync(List<InBoxMessage> inBoxMessageBatch, CancellationToken cancellationToken);
}

internal sealed class InBoxMessageBatchProcessor : IInBoxMessageBatchProcessor
{
    internal const int MaxRetries = 3;
    private readonly IInBoxMessageStorage _inBoxMessageStorage;

    public InBoxMessageBatchProcessor(IInBoxMessageStorage inBoxMessageStorage)
    {
        _inBoxMessageStorage = inBoxMessageStorage;
    }

    public async Task ProcessBatchAsync(List<InBoxMessage> inBoxMessageBatch, CancellationToken cancellationToken)
    {
        if (inBoxMessageBatch.Count == 0) return;

        for (var retryCounter = 0; retryCounter < MaxRetries; retryCounter++)
        {
            try
            {
                await _inBoxMessageStorage.SaveAsync(inBoxMessageBatch.ToArray(), cancellationToken);
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                if (retryCounter == MaxRetries - 1)
                {
                    throw;
                }

                await Task.Delay(1000 * (retryCounter + 1), cancellationToken);
            }
        }
    }
}