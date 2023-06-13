namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Infra.Repositories;
using Messaging;

public struct InBoxMessagesRepositoryFilter
{
    public int BatchSize { get; set; }
    public int MaxNumberAttempts { get; set; }
}

public interface IInBoxMessagesRepository
{
    string Receiver { get; }

    Task MarkMessageAsFailedAsync(InBoxMessage message, CancellationToken cancellationToken = default);

    Task MarkMessageAsProcessedAsync(InBoxMessage message, CancellationToken cancellationToken = default);

    Task<InBoxMessage[]> GetUnprocessedMessagesAsync(InBoxMessagesRepositoryFilter filter, CancellationToken cancellationToken = default);

    Task SaveAsync(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default);
}

public sealed class InBoxMessagesRepository : BoxMessagesRepository, IInBoxMessagesRepository
{
    private string _receiver;

    public InBoxMessagesRepository(IConnectionManager connectionManager)
        : base(connectionManager)
    {
    }

    public string Receiver
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_receiver)) _receiver = Environment.MachineName;
            return _receiver;
        }
    }

    public Task SaveAsync(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = messageEnvelope.Body != null ? Encoding.UTF8.GetString(messageEnvelope.Body) : null;

            var fullName = $"Play.Inventory.Service.Subscribers.Messages.{messageEnvelope.EventName}";
            var inboxMessage = new InBoxMessage(messageEnvelope.EnvelopeId, Processor.Value, messageEnvelope.PubSubName, messageEnvelope.EventName, messageEnvelope.TopicName, payload, Receiver, fullName);
            return InternalSaveAsync(inboxMessage, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task<InBoxMessage[]> GetUnprocessedMessagesAsync(InBoxMessagesRepositoryFilter filter, CancellationToken cancellationToken = default)
    {
        const string sql = InBoxMessagesStatements.GetUnprocessedMessagesAsync;

        cancellationToken.ThrowIfCancellationRequested();

        var parameters = new
        {
            ProcessorId = Processor.Value,
            filter.BatchSize,
            NumberAttempts = filter.MaxNumberAttempts
        };

        return InternalGetUnprocessedMessagesAsync(sql, parameters, cancellationToken);
    }

    public Task MarkMessageAsFailedAsync(InBoxMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task MarkMessageAsProcessedAsync(InBoxMessage message, CancellationToken cancellationToken = default)
    {
        return InternalMarkMessageAsProcessedAsync(message, cancellationToken);
    }

    private async Task InternalMarkMessageAsProcessedAsync(InBoxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var conn = await ConnectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(InBoxMessagesStatements.MarkMessageAsProcessedAsync,
                new
                {
                    Id = message.MessageId,
                    Status = InBoxMessage.InBoxMessageStatus.Processed,
                    CompletedAt = DateTime.UtcNow
                });
            // await RegisterFollowUpAsync(conn, message, "Processed");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task<InBoxMessage[]> InternalGetUnprocessedMessagesAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        try
        {
            await using var conn = await ConnectionManager.GetOpenConnectionAsync(cancellationToken);

            var result = await conn.QueryAsync<InBoxMessageData>(sql, parameters);
            return result.Select(x => x.ToInBoxMessage()).ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task InternalSaveAsync(InBoxMessage inboxMessage, CancellationToken cancellationToken)
    {
        const string receivedStatus = "Received";
        const string sql = InBoxMessagesStatements.SaveAsync;

        cancellationToken.ThrowIfCancellationRequested();

        var inBoxMessageData = inboxMessage.ToInBoxMessageData();

        await using var conn = await ConnectionManager.GetOpenConnectionAsync(cancellationToken);

        await conn.ExecuteAsync(sql, inBoxMessageData);
        // await RegisterFollowUpAsync(conn, inboxMessage, receivedStatus);
    }
}