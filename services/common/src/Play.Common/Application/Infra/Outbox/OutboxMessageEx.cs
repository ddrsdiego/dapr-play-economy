namespace Play.Common.Application.Infra.Outbox;

using System;

public static class OutboxMessageEx
{
    public static OutboxMessageData ToOutboxMessageData(this OutBoxMessage outBoxMessage)
    {
        var outboxMessageData = new OutboxMessageData
        {
            Id = outBoxMessage.Id,
            PubSubName = outBoxMessage.PubSubName,
            ProcessorId = outBoxMessage.ProcessorId,
            EventName = outBoxMessage.EventName,
            TopicName = outBoxMessage.TopicName,
            FullName = outBoxMessage.FullName,
            Payload = outBoxMessage.Payload,
            Status = outBoxMessage.Status,
            Type = OutBoxMessage.BoxType,
            CreatedAt = outBoxMessage.CreatedAt
        };

        return outboxMessageData;
    }

    public static OutBoxMessage ToOutboxMessage(this OutboxMessageData outboxMessageData)
    {
        try
        {
            var outBoxMessage = new OutBoxMessage(outboxMessageData.Id, outboxMessageData.ProcessorId,
                outboxMessageData.PubSubName, outboxMessageData.EventName, outboxMessageData.TopicName,
                outboxMessageData.Status, outboxMessageData.Payload, outboxMessageData.FullName,
                outboxMessageData.NumberAttempts);

            return outBoxMessage;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}