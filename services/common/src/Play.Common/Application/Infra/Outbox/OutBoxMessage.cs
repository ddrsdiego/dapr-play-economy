namespace Play.Common.Application.Infra.Outbox;

using System;

public sealed class OutBoxMessage : BoxMessage
{
    public const string BoxType = "OUT";
    
    private static class OutBoxMessageStatus
    {
        public const string Pending = nameof(Pending);
        public const string Published = nameof(Published);
        public const string Failed = nameof(Failed);
    }

    public OutBoxMessage(string eventName, string topicName, string payload)
        : this(Guid.NewGuid().ToString(), eventName, topicName, OutBoxMessageStatus.Pending, payload)
    {
    }

    internal OutBoxMessage(string id, string eventName, string topicName, string status, string payload)
        : base(id, eventName, topicName, status, payload, BoxType)
    {
    }

    public OutBoxMessage ToMessagePublished() => new(Id, EventName, TopicName, OutBoxMessageStatus.Published, Payload);
}