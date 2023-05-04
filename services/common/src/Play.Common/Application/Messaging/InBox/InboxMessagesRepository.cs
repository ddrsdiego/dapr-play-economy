namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Infra.Repositories;
using Messaging;

public interface IInBoxMessagesRepository
{
    string Receiver { get; }

    public Task SaveAsync(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default);
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

            var inboxMessage = new InBoxMessage(messageEnvelope.EnvelopeId, Processor.Value, messageEnvelope.PubSubName, messageEnvelope.EventName, messageEnvelope.TopicName, payload, Receiver);
            return InternalSaveAsync(inboxMessage, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task SaveAsync(string pubSubName, string eventName, string topicName, string payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var inboxMessage = new InBoxMessage(Guid.NewGuid().ToString(), Processor.Value, pubSubName, eventName, topicName, payload, Receiver);
            return InternalSaveAsync(inboxMessage, cancellationToken);
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
        await RegisterFollowUpAsync(conn, inboxMessage, receivedStatus);
    }
}