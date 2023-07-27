namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Play.Common.Application.Infra.Repositories;
using Play.Common.Application.Messaging.InBox.Extensions;

public interface IInBoxMessageStorageRead : IAsyncDisposable
{
    IAsyncEnumerable<InBoxMessage> GetPendingMessagesAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<InBoxMessage> GetPendingMessagesByPriorityAsync(CancellationToken cancellationToken = default);
}

internal sealed class InBoxMessagePostgresStorageRead : PostgresRepository, IInBoxMessageStorageRead
{
    private static readonly string ProcessorId;
    private readonly Channel<InBoxMessageData> _channel;
    private readonly MessagingSettings _messagingSettings;
    private readonly SemaphoreSlim FetchAndLockSemaphore = new(1, 1);

    static InBoxMessagePostgresStorageRead()
    {
        var podName = Environment.GetEnvironmentVariable("POD_NAME");

        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        var processName = !string.IsNullOrEmpty(podName)?
            podName:
            $"{assemblyName}-{GeneratorOperationId.Generate()}";

        ProcessorId = processName;
    }

    public InBoxMessagePostgresStorageRead(ILoggerFactory logger, MessagingSettings messagingSettings, IConnectionManagerFactory connectionManager)
        : base(logger.CreateLogger<InBoxMessagePostgresStorageRead>(), connectionManager)
    {
        _messagingSettings = messagingSettings;
        var channelOptions = new BoundedChannelOptions(_messagingSettings.InBox.BufferCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
        };

        _channel = Channel.CreateBounded<InBoxMessageData>(channelOptions);
    }

    public async IAsyncEnumerable<InBoxMessage> GetPendingMessagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IEnumerable<InBoxMessageData> resultSet;

        try
        {
            await FetchAndLockSemaphore.WaitAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await using var connectionManager = ConnectionManagerFactory.CreateConnection();
            await using var conn = await connectionManager.GetOpenConnectionWithTransactionAsync(cancellationToken);

            await SetDataBaseTimeZoneAsync(conn, cancellationToken: cancellationToken);

            resultSet = await conn.QueryAsync<InBoxMessageData>(InBoxMessageStatements.GetPendingMessages, new
            {
                ProcessorId,
                _messagingSettings.InBox.BatchSize,
                _messagingSettings.InBox.NumberAttempts,
                Status = InBoxMessage.InBoxMessageStatus.InProgress,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            FetchAndLockSemaphore.Release();
        }

        foreach (var messageIntegrationData in resultSet)
        {
            yield return messageIntegrationData.ToInBoxMessageData(ProcessorId);
        }
    }

    public async IAsyncEnumerable<InBoxMessage> GetPendingMessagesByPriorityAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var inBoxMessageData in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return inBoxMessageData.ToInBoxMessageData(ProcessorId);
        }
    }

    private async Task ConsumerTask(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(5_000), cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            await InternalGetPendingMessagesByPriorityAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromMilliseconds(_messagingSettings.InBox.IntervalTimeFetchMessage), cancellationToken);
        }
    }

    private async Task<IEnumerable<InBoxMessageData>> GetInBoxMessageByPriority(InBoxMessagePriority priority, CancellationToken cancellationToken = default)
    {
        await using var connectionManager = ConnectionManagerFactory.CreateConnection();
        await using var conn = await connectionManager.GetOpenConnectionWithTransactionAsync(cancellationToken);

        await SetDataBaseTimeZoneAsync(conn, cancellationToken: cancellationToken);

        var inBoxMessagesData = await conn.QueryAsync<InBoxMessageData>(InBoxMessageStatements.GetMessagesWithPriorityPending, new
        {
            _messagingSettings.InBox.NumberAttempts,
            ProcessorId = GeneratorOperationId.Generate(),
            NextStatus = InBoxMessage.InBoxMessageStatus.InProgress,
            Priority = priority.Value,
            _messagingSettings.InBox.BatchSize,
        });

        return inBoxMessagesData;
    }

    private async Task InternalGetPendingMessagesByPriorityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await FetchAndLockSemaphore.WaitAsync(cancellationToken);

            await DispatchMessagesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            FetchAndLockSemaphore.Release();
        }
    }

    private async Task DispatchMessagesAsync(CancellationToken cancellationToken)
    {
        var priorities = InBoxMessagePriority.PriorityFromHighToNoneCache;

        var messagesRemain = true;

        while (messagesRemain)
        {
            messagesRemain = false;

            for (var priorityCounter = 0; priorityCounter < priorities.Length; priorityCounter++)
            {
                var currentPriority = priorities[priorityCounter];

                if (priorityCounter > 0)
                {
                    for (var highPriorityCounter = 0; highPriorityCounter < priorityCounter; highPriorityCounter++)
                    {
                        var highPriority = priorities[highPriorityCounter];
                        if (await DispatchMessagesByPriority(highPriority, cancellationToken))
                            messagesRemain = true;
                    }
                }

                if (await DispatchMessagesByPriority(currentPriority, cancellationToken))
                    messagesRemain = true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
    }

    private async Task<bool> DispatchMessagesByPriority(InBoxMessagePriority priority, CancellationToken cancellationToken)
    {
        var messagesProcessed = false;

        while (true)
        {
            var inBoxMessagesData = await GetInBoxMessageByPriority(priority, cancellationToken);
            var messages = inBoxMessagesData as InBoxMessageData[] ?? inBoxMessagesData.ToArray();

            if (messages.Length == 0)
                break;

            if (await _channel.Writer.WaitToWriteAsync(cancellationToken))
            {
                for (var index = 0; index < messages.Length; index++)
                {
                    await _channel.Writer.WriteAsync(messages[index], cancellationToken);
                    messagesProcessed = true;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        return messagesProcessed;
    }

    private async Task DispatchMessagesByPriorityAsync(InBoxMessagePriority priority, CancellationToken cancellationToken)
    {
        var inBoxMessagesData = await GetInBoxMessageByPriority(priority, cancellationToken);
        var messages = inBoxMessagesData as InBoxMessageData[] ?? inBoxMessagesData.ToArray();

        if (await _channel.Writer.WaitToWriteAsync(cancellationToken))
        {
            for (var index = 0; index < messages.Length; index++)
            {
                await _channel.Writer.WriteAsync(messages[index], cancellationToken);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            _channel.Writer.TryComplete();
            FetchAndLockSemaphore.Dispose();
        }
        catch (OperationCanceledException)
        {
            // Ignore this exception as we've just canceled the consumer task
        }

        return ValueTask.CompletedTask;
    }
}