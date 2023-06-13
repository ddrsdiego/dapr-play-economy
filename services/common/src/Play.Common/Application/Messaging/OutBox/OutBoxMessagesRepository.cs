namespace Play.Common.Application.Messaging.OutBox;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Extensions;
using Infra.Repositories;
using Messaging;

public struct OutBoxMessagesRepositoryFilter
{
    public int BatchSize { get; set; }
    public int NumberAttempts { get; set; }
}

public interface IOutBoxMessagesRepository
{
    string Sender { get; }

    IAsyncEnumerable<OutBoxMessage> GetMessagesPendingToPublishAsync(OutBoxMessagesRepositoryFilter filter, CancellationToken cancellationToken = default);

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
    Task SaveAsync(string pubSubName, string eventName, string topicName, object payload, CancellationToken cancellationToken = default);

    Task IncrementNumberAttemptsAsync(OutBoxMessage outBoxMessage, string errorMessage, CancellationToken cancellationToken = default);
}

public sealed class OutBoxMessagesRepository : BoxMessagesRepository, IOutBoxMessagesRepository
{
    private string _sender;

    public OutBoxMessagesRepository(IConnectionManager connectionManager)
        : base(connectionManager)
    {
    }

    public string Sender
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_sender)) _sender = Environment.GetEnvironmentVariable("APP_ID");
            return _sender;
        }
    }

    public async IAsyncEnumerable<OutBoxMessage> GetMessagesPendingToPublishAsync(OutBoxMessagesRepositoryFilter filter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const string sql = OutBoxMessagesStatements.GetUnprocessedAsync;

        cancellationToken.ThrowIfCancellationRequested();

        await using var conn = await ConnectionManager.GetOpenConnectionAsync(cancellationToken);
        var resultSet = await conn.QueryAsync<OutBoxMessageData>(sql,
            new
            {
                ProcessorId = Processor.Value,
                NumberAttempts = filter.NumberAttempts,
                BatchSize = filter.BatchSize,
                Status = OutBoxMessage.OutBoxMessageStatus.Processing
            });

        foreach (var outboxMessageData in resultSet)
        {
            yield return outboxMessageData.ToOutboxMessage();
        }
    }

    public async Task UpdateToPublishedAsync(OutBoxMessage outBoxMessages, CancellationToken cancellationToken = default)
    {
        const string publishedStatusFollowUp = "Published";
        const string sql = OutBoxMessagesStatements.UpdateToPublishedAsync;

        cancellationToken.ThrowIfCancellationRequested();

        var outboxMessagePublished = outBoxMessages.ToMessagePublished();

        await using var conn = await ConnectionManager.GetOpenConnectionAsync(cancellationToken);
        await conn.ExecuteAsync(sql,
            new
            {
                outboxMessagePublished.MessageId,
                outboxMessagePublished.Status,
                SentAt = DateTimeOffset.UtcNow
            });
        await RegisterFollowUpAsync(conn, outboxMessagePublished, publishedStatusFollowUp);
    }

    public Task SaveAsync(string pubSubName, string eventName, string topicName, object payload, CancellationToken cancellationToken = default)
    {
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var outboxMessage = new OutBoxMessage(pubSubName, eventName, topicName, payloadJson, payload.GetType().FullName)
        {
            Sender = Sender,
            ProcessorId = Processor.Value,
            Status = OutBoxMessage.OutBoxMessageStatus.Pending
        };

        return InternalSaveAsync(outboxMessage, cancellationToken);
    }

    public async Task IncrementNumberAttemptsAsync(OutBoxMessage outBoxMessage, string errorMessage,
        CancellationToken cancellationToken = default)
    {
        const string failedStatusFollowUp = "Failed";
        const string sql = OutBoxMessagesStatements.IncrementNumberAttempts;
        try
        {
            outBoxMessage.IncrementNumberAttempts();

            await using var conn = await ConnectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(sql,
                new
                {
                    Id = outBoxMessage.MessageId,
                    outBoxMessage.NumberAttempts
                });
            await RegisterFollowUpAsync(conn, outBoxMessage, failedStatusFollowUp, errorMessage);
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

            var outboxMessageData = outBoxMessage
                .ToOutboxMessageData();

            await using var conn = await ConnectionManager.GetOpenConnectionAsync(cancellationToken);

            await conn.ExecuteAsync(OutBoxMessagesStatements.SaveAsync, outboxMessageData);
            await RegisterFollowUpAsync(conn, outBoxMessage, createdStatusFollowUp);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}