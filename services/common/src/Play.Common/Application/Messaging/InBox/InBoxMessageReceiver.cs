namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public interface IInBoxMessageReceiver : IAsyncDisposable
{
    Task CompleteAsync { get; }

    ValueTask EnqueueMessageAsync(InBoxMessage inBoxMessage, CancellationToken cancellationToken = default);
}

internal sealed class InBoxMessageReceiver : IInBoxMessageReceiver
{
    private const int BatchCapacity = 1_000;

    private readonly IInBoxMessageBatchProcessor _batchProcessor;
    private readonly IImmutableDictionary<int, Channel<InBoxMessage>> _channels = ImmutableDictionary<int, Channel<InBoxMessage>>.Empty;

    public InBoxMessageReceiver(IHostApplicationLifetime applicationLifetime, IInBoxMessageBatchProcessor batchProcessor)
    {
        _batchProcessor = batchProcessor;

        foreach (var priority in InBoxMessagePriority.PriorityFromHighToNoneCache)
        {
            if (priority.Value == InBoxMessagePriority.High.Value)
            {
                _channels = _channels.Add(priority.Value, CreateUnboundedChannel());
            }
            else
            {
                _channels = _channels.Add(priority.Value, CreateBoundedChannel());
            }
        }

        CompleteAsync = Task.Run(() => ChannelReaderMonitorAsync(applicationLifetime.ApplicationStopping));
    }

    public Task CompleteAsync { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask EnqueueMessageAsync(InBoxMessage inBoxMessage, CancellationToken cancellationToken = default)
    {
        if (_channels.TryGetValue(inBoxMessage.Priority, out var channel))
        {
            return channel.Writer.WriteAsync(inBoxMessage, cancellationToken);
        }

        return _channels[InBoxMessagePriority.None.Value].Writer.WriteAsync(inBoxMessage, cancellationToken);
    }

    private async Task ChannelReaderMonitorAsync(CancellationToken cancellationToken)
    {
        var maxPriorityValue = InBoxMessagePriority.MaxPriority;

        var inBoxMessageBatch = new List<InBoxMessage>(BatchCapacity);

        while (!cancellationToken.IsCancellationRequested)
        {
            inBoxMessageBatch.Clear();

            for (var priority = maxPriorityValue; priority >= 0; priority--)
            {
                if (!_channels.TryGetValue(priority, out var channel)) continue;

                while (inBoxMessageBatch.Count < BatchCapacity && channel.Reader.TryRead(out var inBoxMessage))
                {
                    inBoxMessageBatch.Add(inBoxMessage);
                }

                if (inBoxMessageBatch.Count >= BatchCapacity) break;
            }

            try
            {
                await _batchProcessor.ProcessBatchAsync(inBoxMessageBatch, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                await Task.Delay(10, cancellationToken);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var channel in _channels.Values)
        {
            channel.Writer.TryComplete();
        }

        await CompleteAsync;
        CompleteAsync.Dispose();
    }

    private static Channel<InBoxMessage> CreateBoundedChannel(int capacity = 1024 * 1024 * 2) => Channel.CreateBounded<InBoxMessage>(new BoundedChannelOptions(capacity)
    {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false,
        FullMode = BoundedChannelFullMode.Wait,
    });

    private static Channel<InBoxMessage> CreateUnboundedChannel() => Channel.CreateUnbounded<InBoxMessage>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
        AllowSynchronousContinuations = false
    });
}