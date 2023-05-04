namespace Play.Common.Application.Messaging.InBox;

using Messaging;

public sealed class InBoxMessage : BoxMessage
{
    public string Receiver { get; }

    public string ProcessorId { get; }

    private static class InBoxMessageStatus
    {
        public const string Pending = nameof(Pending);
        public const string Processed = nameof(Processed);
        public const string Failed = nameof(Failed);
    }

    public const string BoxType = "IN";

    public InBoxMessage(string messageId, string processorId, string pubSubName, string eventName, string topicName, string payload, string receiver)
        : base(messageId, pubSubName, eventName, topicName, InBoxMessageStatus.Pending, payload, string.Empty, BoxType, 0)
    {
        Receiver = receiver;
        ProcessorId = processorId;
    }
}