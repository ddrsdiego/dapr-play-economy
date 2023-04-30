namespace Play.Common.Application.Infra.Outbox;

using System;

public abstract class BoxMessage
{
    protected BoxMessage(string id, string eventName, string topicName, string status, string payload, string type)
    {
        Id = id;
        EventName = eventName;
        TopicName = topicName;
        Status = status;
        Payload = payload;
        Type = type;
        FullName = payload.GetType().FullName;
    }

    public string Id { get; }
    public string EventName { get; }
    public string FullName { get; }
    public string TopicName { get; }
    public string Status { get; }
    public string Payload { get; }
    public string Type { get; }

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}