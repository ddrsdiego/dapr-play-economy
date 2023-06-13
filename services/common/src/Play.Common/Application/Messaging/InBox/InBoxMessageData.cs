namespace Play.Common.Application.Messaging.InBox;

using System;

public struct InBoxMessageData
{
    public string MessageId { get; set; }
    public string ProcessorId { get; set; }
    public string PubSubName { get; set; }
    public string EventName { get; set; }
    public string TopicName { get; set; }
    public string Status { get; set; }
    public int NumberAttempts { get; set; }
    public string FullName { get; set; }
    public string Payload { get; set; }
    public string BoxType { get; set; }
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public string MachineReceiver { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public static class InBoxMessageEx
{
    public static InBoxMessage ToInBoxMessage(this InBoxMessageData data)
    {
        return new InBoxMessage(data.MessageId, data.ProcessorId, data.PubSubName, data.EventName, data.TopicName, data.Payload, data.Receiver, data.FullName);
    }

    public static InBoxMessageData ToInBoxMessageData(this InBoxMessage inBoxMessage)
    {
        return new InBoxMessageData
        {
            MessageId = inBoxMessage.MessageId,
            EventName = inBoxMessage.EventName,
            TopicName = inBoxMessage.TopicName,
            FullName = inBoxMessage.FullName,
            Payload = inBoxMessage.Payload,
            Status = InBoxMessage.InBoxMessageStatus.Pending,
            CreatedAt = inBoxMessage.CreatedAt,
            BoxType = InBoxMessage.BoxType,
            PubSubName = inBoxMessage.PubSubName,
            Sender = inBoxMessage.Receiver,
            ProcessorId = inBoxMessage.ProcessorId,
        };
    }
}