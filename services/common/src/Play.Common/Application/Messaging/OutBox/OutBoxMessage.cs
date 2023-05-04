namespace Play.Common.Application.Messaging.OutBox;

using System;
using Messaging;

public sealed class OutBoxMessage : BoxMessage
{
    public const string BoxType = "OUT";

    private static class OutBoxMessageStatus
    {
        public const string Pending = nameof(Pending);
        public const string Published = nameof(Published);
        public const string Failed = nameof(Failed);
    }

    public OutBoxMessage(string processorId, string sender, string pubSubName, string eventName, string topicName, string payload, string fullName)
        : this(Guid.NewGuid().ToString(), processorId, sender, pubSubName, eventName, topicName, OutBoxMessageStatus.Pending, payload, fullName)
    {
    }

    internal OutBoxMessage(string messageId, string processorId, string sender, string pubSubName, string eventName, string topicName, string status, string payload, string fullName, int numberAttempts = 0)
        : base(messageId, pubSubName, eventName, topicName, status, payload, fullName, BoxType, numberAttempts)
    {
        ProcessorId = processorId;
        Sender = sender;
    }

    public string ProcessorId { get; }
    public string Sender { get; }

    public OutBoxMessage ToMessageFailed() => new(MessageId, ProcessorId, Sender, PubSubName, EventName, TopicName, OutBoxMessageStatus.Failed, Payload, FullName, NumberAttempts);
    
    public OutBoxMessage ToMessagePublished() => new(MessageId, ProcessorId, Sender, PubSubName, EventName, TopicName, OutBoxMessageStatus.Published, Payload, FullName, NumberAttempts);

    public void IncrementNumberAttempts() => NumberAttempts++;
}