namespace Play.Inventory.Service.Workers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public interface IProcessorChannel<in T> : IAsyncDisposable
{
    ValueTask EnqueueAsync(T message, CancellationToken stoppingToken);
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

public sealed class ProcessorChannel<T> : IProcessorChannel<T>
{
    private Task _channelTask;
    private readonly int _maxBatchSize;
    private readonly Func<T, Task> _method;
    private readonly Channel<T> _queue;
    private readonly ProcessorChannelId _channelId;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static IProcessorChannel<T> Create(string channelId, Func<T, Task> method) => new ProcessorChannel<T>(channelId, 100, 1, method);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="capacity"></param>
    /// <param name="maxBatchSize"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static IProcessorChannel<T> Create(string channelId, int capacity, int maxBatchSize, Func<T, Task> method) => new ProcessorChannel<T>(channelId, capacity, maxBatchSize, method);

    private ProcessorChannel(string channelId, int capacity, int maxBatchSize, Func<T, Task> method)
    {
        _maxBatchSize = maxBatchSize;
        _method = method ?? throw new ArgumentNullException(nameof(method));

        var channelOptions = new BoundedChannelOptions(capacity)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _queue = Channel.CreateBounded<T>(channelOptions);
        _channelId = new ProcessorChannelId(channelId);

        _channelTask = Task.Run(StartConsumerAsync);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask EnqueueAsync(T message, CancellationToken stoppingToken) => _queue.Writer.WriteAsync(message, stoppingToken);

    private async Task StartConsumerAsync()
    {
        Console.WriteLine($"Channel: {_channelId} ready for initiate consumption");

        try
        {
            while (await _queue.Reader.WaitToReadAsync())
            {
                var counter = 0;
                var batch = new List<T>(_maxBatchSize);

                var sw = Stopwatch.StartNew();

                try
                {
                    while (counter < _maxBatchSize && _queue.Reader.TryRead(out var message))
                    {
                        batch.Add(message);
                        counter++;
                    }

                    var enumerableTasks = batch.ToArray();
                    for (var index = 0; index < enumerableTasks.Length; index++)
                    {
                        var message = enumerableTasks[index];
                        try
                        {
                            await _method(message).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
                finally
                {
                    Console.WriteLine($"{batch.Count} - {sw.ElapsedMilliseconds}");

                    batch.Clear();
                    batch = null;
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

    public async ValueTask DisposeAsync()
    {
        _queue.Writer.TryComplete();

        await _channelTask;

        _channelTask.Dispose();
        _channelTask = null;
    }
}