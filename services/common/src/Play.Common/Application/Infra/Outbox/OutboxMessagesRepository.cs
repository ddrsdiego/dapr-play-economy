namespace Play.Common.Application.Infra.Outbox;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Repositories;

public interface IOutboxMessagesRepository
{
    Task<OutBoxMessage[]> GetMessagesPendingToPublishAsync(
        CancellationToken cancellationToken = default);

    Task UpdateToPublishedAsync(OutBoxMessage outBoxMessages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record an outbox message to be published later.
    /// </summary>
    /// <param name="eventName">Event Name</param>
    /// <param name="topicName">Topic to be </param>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveAsync(string eventName, string topicName, object payload, CancellationToken cancellationToken = default);

    Task IncrementNumberAttemptsAsync(OutBoxMessage outBoxMessage, CancellationToken cancellationToken);
}

public sealed class OutboxMessagesRepository : Repository, IOutboxMessagesRepository
{
    private readonly IConnectionManager _connectionManager;

    public OutboxMessagesRepository(ILoggerFactory logger, IConnectionManager connectionManager)
        : base(logger.CreateLogger<OutboxMessagesRepository>(), connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<OutBoxMessage[]> GetMessagesPendingToPublishAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);

            var resultSet = await conn.QueryAsync<OutboxMessageData>(
                OutboxMessagesStatements.GetUnprocessedAsync);

            var outboxMessagesData = resultSet as OutboxMessageData[] ?? resultSet.ToArray();

            return !outboxMessagesData.Any()
                ? Array.Empty<OutBoxMessage>()
                : outboxMessagesData.Select(x => x.ToOutboxMessage()).ToArray();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while getting unprocessed outbox messages");
            return Array.Empty<OutBoxMessage>();
        }
    }

    public async Task UpdateToPublishedAsync(OutBoxMessage outBoxMessages,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var outboxMessagePublished = outBoxMessages.ToMessagePublished();

            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);

            await conn.ExecuteAsync(OutboxMessagesStatements.UpdateToPublishedAsync,
                new
                {
                    outboxMessagePublished.Id,
                    outboxMessagePublished.Status,
                    SentAt = DateTimeOffset.UtcNow
                });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public Task SaveAsync(string eventName, string topicName, object payload,
        CancellationToken cancellationToken = default)
    {
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var outboxMessage = new OutBoxMessage(eventName, topicName, payloadJson, payload.GetType().FullName);

        return InternalSaveAsync(outboxMessage, cancellationToken);
    }

    public async Task IncrementNumberAttemptsAsync(OutBoxMessage outBoxMessage, CancellationToken cancellationToken)
    {
        try
        {
            outBoxMessage.IncrementNumberAttempts();
        
            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(OutboxMessagesStatements.IncrementNumberAttempts,
                new
                {
                    outBoxMessage.Id,
                    outBoxMessage.NumberAttempts
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task InternalSaveAsync(OutBoxMessage outBoxMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(OutboxMessagesStatements.SaveAsync,
                outBoxMessage.ToOutboxMessageData());
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while saving outbox message");
            throw;
        }
    }
}