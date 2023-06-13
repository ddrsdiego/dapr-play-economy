namespace Play.Common.Application.Messaging.OutBox;

using System;

public sealed class OutBoxMessage : BoxMessage
{
    public const string BoxType = "OUT";

    public static class OutBoxMessageStatus
    {
        public const string Pending = nameof(Pending);
        public const string Published = nameof(Published);
        public const string Processing = nameof(Processing);
        public const string Failed = nameof(Failed);
    }

    public OutBoxMessage(string pubSubName, string eventName, string topicName, string payload, string fullName)
        : base(Guid.NewGuid().ToString(), pubSubName, eventName, topicName, payload, fullName)
    {
    }

    internal OutBoxMessage(string messageId, string pubSubName, string eventName, string topicName, string payload, string fullName)
        : base(messageId, pubSubName, eventName, topicName, payload, fullName)
    {
    }

    public string ProcessorId { get; set; }
    public string Sender { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public int NumberAttempts { get; set; }
    public DateTimeOffset PublishedAt { get; set; }

    public void IncrementNumberAttempts() => NumberAttempts++;

    public OutBoxMessage ToMessagePublished()
    {
        PublishedAt = DateTimeOffset.Now;
        Status = OutBoxMessageStatus.Published;
        return this;
    }

    public OutBoxMessage ToMessageFailed()
    {
        Status = OutBoxMessageStatus.Failed;
        return this;
    }
}