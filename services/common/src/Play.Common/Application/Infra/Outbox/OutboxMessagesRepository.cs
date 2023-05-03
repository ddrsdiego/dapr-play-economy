namespace Play.Common.Application.Infra.Outbox;

using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Repositories;

public sealed class MessagesProcessorId
{
    public string Value { get; }

    public MessagesProcessorId(string value) => Value = value;
}

public interface IOutboxMessagesRepository
{
    Task<OutBoxMessage[]> GetMessagesPendingToPublishAsync(CancellationToken cancellationToken = default);

    Task UpdateToPublishedAsync(OutBoxMessage outBoxMessages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record an outbox message to be published later.
    /// </summary>
    /// <param name="pubSubName"></param>
    /// <param name="eventName">Event Name</param>
    /// <param name="topicName">Topic to be </param>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveAsync(string pubSubName, string eventName, string topicName, object payload,
        CancellationToken cancellationToken = default);

    Task IncrementNumberAttemptsAsync(OutBoxMessage outBoxMessage, string errorMessage,
        CancellationToken cancellationToken = default);
}

public sealed class OutboxMessagesRepository : Repository, IOutboxMessagesRepository
{
    private readonly DaprClient _daprClient;
    private readonly IConnectionManager _connectionManager;

    public OutboxMessagesRepository(ILoggerFactory logger, DaprClient daprClient, IConnectionManager connectionManager)
        : base(logger.CreateLogger<OutboxMessagesRepository>(), connectionManager)
    {
        _daprClient = daprClient;
        _connectionManager = connectionManager;
        Processor = new MessagesProcessorId(Guid.NewGuid().ToString());
    }

    private MessagesProcessorId Processor { get; }

    public async Task<OutBoxMessage[]> GetMessagesPendingToPublishAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = OutboxMessagesStatements.GetUnprocessedAsync;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var fileLock = await _daprClient.Lock("play-customer-lockstore", "outbox-messages", Processor.Value, 5,
                cancellationToken: cancellationToken);

            if (!fileLock.Success)
                return Array.Empty<OutBoxMessage>();

            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            var resultSet = await conn.QueryAsync<OutboxMessageData>(sql,
                new
                {
                    ProcessorId = Processor.Value
                });

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
        finally
        {
            await _daprClient.Unlock("play-customer-lockstore", "outbox-messages", Processor.Value,
                cancellationToken: cancellationToken);
        }
    }

    public async Task UpdateToPublishedAsync(OutBoxMessage outBoxMessages,
        CancellationToken cancellationToken = default)
    {
        const string publishedStatusFollowUp = "Published";
        const string sql = OutboxMessagesStatements.UpdateToPublishedAsync;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var outboxMessagePublished = outBoxMessages.ToMessagePublished();

            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(sql,
                new
                {
                    outboxMessagePublished.Id,
                    outboxMessagePublished.Status,
                    SentAt = DateTimeOffset.UtcNow
                });
            await RegisterFollowUpAsync(conn, outboxMessagePublished, publishedStatusFollowUp);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public Task SaveAsync(string pubSubName, string eventName, string topicName, object payload,
        CancellationToken cancellationToken = default)
    {
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var outboxMessage =
            new OutBoxMessage(Processor.Value, pubSubName, eventName, topicName, payloadJson,
                payload.GetType().FullName);

        return InternalSaveAsync(outboxMessage, cancellationToken);
    }

    public async Task IncrementNumberAttemptsAsync(OutBoxMessage outBoxMessage, string errorMessage,
        CancellationToken cancellationToken = default)
    {
        const string failedStatusFollowUp = "Failed";
        const string sql = OutboxMessagesStatements.IncrementNumberAttempts;
        try
        {
            outBoxMessage.IncrementNumberAttempts();

            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(sql,
                new
                {
                    outBoxMessage.Id,
                    outBoxMessage.NumberAttempts
                });
            await RegisterFollowUpAsync(conn, outBoxMessage, failedStatusFollowUp, errorMessage);

            Logger.LogInformation("Incremented number of attempts for outbox message {Id}", outBoxMessage.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task InternalSaveAsync(OutBoxMessage outBoxMessage, CancellationToken cancellationToken = default)
    {
        const string createdStatusFollowUp = "Created";

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var outboxMessageData = outBoxMessage.ToOutboxMessageData();

            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(OutboxMessagesStatements.SaveAsync, outboxMessageData);
            await RegisterFollowUpAsync(conn, outBoxMessage, createdStatusFollowUp);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while saving outbox message");
            throw;
        }
    }

    private static Task RegisterFollowUpAsync(IDbConnection conn, BoxMessage outboxMessagePublished, string status,
        string errorMessage = "")
    {
        const string sql = OutboxMessagesStatements.SaveFollowUpAsync;
        return conn.ExecuteAsync(sql,
            new
            {
                BoxMessagesId = outboxMessagePublished.Id,
                Status = status,
                UpdatedAt = DateTimeOffset.UtcNow,
                Exception = !string.IsNullOrWhiteSpace(errorMessage) ? errorMessage : null
            });
    }
}