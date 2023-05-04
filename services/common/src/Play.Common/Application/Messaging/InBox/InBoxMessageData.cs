namespace Play.Common.Application.Messaging.InBox;

using System;

public struct InBoxMessageData
{
    public string Id { get; set; }
    public string ProcessorId { get; set; }
    public string EventName { get; set; }
    public string TopicName { get; set; }
    public string FullName { get; set; }
    public string Payload { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public int NumberAttempts { get; set; }
    public string Sender { get; set; }
    public string PubSubName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public static class InBoxMessageEx
{
    public static InBoxMessageData ToInBoxMessageData(this InBoxMessage inBoxMessage)
    {
        return new InBoxMessageData
        {
            Id = inBoxMessage.MessageId,
            EventName = inBoxMessage.EventName,
            TopicName = inBoxMessage.TopicName,
            FullName = inBoxMessage.FullName,
            Payload = inBoxMessage.Payload,
            Status = inBoxMessage.Status,
            CreatedAt = inBoxMessage.CreatedAt,
            Type = InBoxMessage.BoxType,
            PubSubName = inBoxMessage.PubSubName,
            Sender = inBoxMessage.Receiver,
            ProcessorId = inBoxMessage.ProcessorId,
        };
    }
}