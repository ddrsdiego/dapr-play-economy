namespace Play.Common.Application.Messaging.OutBox.Extensions;

using System;
using System.Text;

public static class OutBoxMessageEx
{
    public static MessageEnvelope ToEnvelopeMessage(this OutBoxMessage outBoxMessage)
    {
        var body = Encoding.UTF8.GetBytes(outBoxMessage.Payload);
        return new MessageEnvelope(outBoxMessage.MessageId, outBoxMessage.PubSubName, outBoxMessage.EventName, outBoxMessage.TopicName, outBoxMessage.Sender, body);
    }

    public static OutBoxMessageData ToOutboxMessageData(this OutBoxMessage outBoxMessage)
    {
        var outboxMessageData = new OutBoxMessageData
        {
            Id = outBoxMessage.MessageId,
            Sender = outBoxMessage.Sender,
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

    public static OutBoxMessage ToOutboxMessage(this OutBoxMessageData outBoxMessageData)
    {
        try
        {
            var outBoxMessage = new OutBoxMessage(outBoxMessageData.Id, outBoxMessageData.ProcessorId,
                outBoxMessageData.Sender, outBoxMessageData.PubSubName, outBoxMessageData.EventName,
                outBoxMessageData.TopicName, outBoxMessageData.Status, outBoxMessageData.Payload,
                outBoxMessageData.FullName, outBoxMessageData.NumberAttempts);

            return outBoxMessage;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}