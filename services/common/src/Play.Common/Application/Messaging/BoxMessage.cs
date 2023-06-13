namespace Play.Common.Application.Messaging;

using System;

public abstract class BoxMessage
{
    protected BoxMessage(string messageId, string pubSubName, string eventName, string topicName, string payload, string fullName)
    {
        MessageId = messageId;
        PubSubName = pubSubName;
        EventName = eventName;
        TopicName = topicName;
        Payload = payload;
        FullName = fullName;
    }

    public string MessageId { get; }
    public string EventName { get; }
    public string FullName { get; }
    public string TopicName { get; }
    public string Payload { get; }
    public string PubSubName { get; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}