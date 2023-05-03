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

    public OutBoxMessage(string processorId, string pubSubName, string eventName, string topicName, string payload,
        string fullName)
        : this(Guid.NewGuid().ToString(), processorId, pubSubName, eventName, topicName, OutBoxMessageStatus.Pending,
            payload, fullName, 0)
    {
    }

    internal OutBoxMessage(string id, string processorId, string pubSubName, string eventName, string topicName,
        string status, string payload, string fullName, int numberAttempts)
        : base(id, pubSubName, eventName, topicName, status, payload, fullName, BoxType, numberAttempts)
    {
        ProcessorId = processorId;
    }

    public string ProcessorId { get; }

    public OutBoxMessage ToMessagePublished() => new(Id, ProcessorId, PubSubName, EventName, TopicName,
        OutBoxMessageStatus.Published, Payload, FullName, NumberAttempts);


    public void IncrementNumberAttempts() => NumberAttempts++;
}