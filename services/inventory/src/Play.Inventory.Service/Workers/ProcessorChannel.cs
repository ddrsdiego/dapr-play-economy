namespace Play.Inventory.Service.Workers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;

public interface IProcessorChannel<in TMessage> : IAsyncDisposable
{
    ValueTask EnqueueAsync(TMessage message, CancellationToken stoppingToken = default);
}

public readonly struct ProcessorChannelId
{
    public ProcessorChannelId(string channelName)
        : this(Guid.NewGuid().ToString(), channelName)
    {
    }

    private ProcessorChannelId(string id, string name) => (Id, Name) = (id, name);

    public string Id { get; }
    public string Name { get; }

    public override string ToString() => $"{Id} - {Name}";
}

public sealed class ProcessorChannel<TMessage> : IProcessorChannel<TMessage>
{
    private Task _channelTask;
    private readonly int _maxBatchSize;
    private readonly Func<ArraySegment<TMessage>, Task> _batchMethod;
    private readonly ILogger _logger;
    private readonly Func<TMessage, Task> _method;
    private readonly Channel<TMessage> _queue;
    private readonly ProcessorChannelId _channelId;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static IProcessorChannel<TMessage> Create(string channelId, Func<TMessage, Task> method) => new ProcessorChannel<TMessage>(channelId, 100, 1, method);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="capacity"></param>
    /// <param name="maxBatchSize"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static IProcessorChannel<TMessage> Create(string channelId, int capacity, int maxBatchSize, Func<TMessage, Task> method) => new ProcessorChannel<TMessage>(channelId, capacity, maxBatchSize, method);

    public ProcessorChannel(int capacity, int maxBatchSize, Func<ArraySegment<TMessage>, Task> batchMethod, ILogger logger)
    {
        _maxBatchSize = maxBatchSize;
        _batchMethod = batchMethod;
        _logger = logger;

        var channelOptions = new BoundedChannelOptions(capacity)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _queue = Channel.CreateBounded<TMessage>(channelOptions);
        _channelTask = Task.Run(StartConsumerAsync);
    }

    private ProcessorChannel(string channelId, int capacity, int maxBatchSize, Func<TMessage, Task> method)
    {
        _maxBatchSize = maxBatchSize;
        _method = method ?? throw new ArgumentNullException(nameof(method));

        var channelOptions = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
        };

        _queue = Channel.CreateBounded<TMessage>(channelOptions);
        _channelId = new ProcessorChannelId(channelId);

        _channelTask = Task.Run(StartConsumerAsync);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask EnqueueAsync(TMessage message, CancellationToken stoppingToken = default)
    {
        var writeTask = _queue.Writer.WriteAsync(message, stoppingToken);

        return writeTask.IsCompletedSuccessfully ? ValueTask.CompletedTask : SlowWrite(writeTask);

        async ValueTask SlowWrite(ValueTask task) => await task;
    }

    private async Task StartConsumerAsync()
    {
        Console.WriteLine($"Channel: {_channelId} ready for initiate consumption");

        try
        {
            while (await _queue.Reader.WaitToReadAsync())
            {
                var sw = Stopwatch.StartNew();

                var counter = 0;
                var batch = new TMessage[_maxBatchSize];

                while (counter < _maxBatchSize && _queue.Reader.TryRead(out var message))
                {
                    batch[counter++] = message;
                }

                var safeTask = SafeExecuteAsync(batch, counter);

                try
                {
                    if (!safeTask.IsCompletedSuccessfully)
                        await safeTask;
                }
                finally
                {
                    Console.WriteLine($"{batch.Length} - {sw.ElapsedMilliseconds}");
                    Array.Clear(batch, 0, counter);
                }
            }
        }
        catch (Exception e)
        {
            _queue.Writer.TryComplete(e);
        }
        finally
        {
            _queue.Writer.TryComplete();
        }
    }

    private Task SafeExecuteAsync(TMessage[] messages, int counter) => _batchMethod(new ArraySegment<TMessage>(messages, 0, counter));

    public async ValueTask DisposeAsync()
    {
        _queue.Writer.TryComplete();

        await _channelTask;

        _channelTask.Dispose();
        _channelTask = null;
    }
}