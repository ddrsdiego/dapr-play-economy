namespace Play.Common.Application.Messaging;

using System;

public abstract class BoxMessage
{
    protected BoxMessage(string messageId, string pubSubName, string eventName, string topicName, string status, string payload, string fullName, string type, int numberAttempts)
    {
        MessageId = messageId;
        PubSubName = pubSubName;
        EventName = eventName;
        TopicName = topicName;
        Status = status;
        Payload = payload;
        Type = type;
        FullName = fullName;
        NumberAttempts = numberAttempts;
    }

    public string MessageId { get; }
    public string EventName { get; }
    public string FullName { get; }
    public string TopicName { get; }
    public string Status { get; }
    public string Payload { get; }
    public string Type { get; }
    public string PubSubName { get;  }
    public int NumberAttempts { get; protected set; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}