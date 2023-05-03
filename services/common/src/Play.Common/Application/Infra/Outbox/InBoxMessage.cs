namespace Play.Common.Application.Infra.Outbox;

public sealed class InBoxMessage : BoxMessage
{
    private static class InBoxMessageStatus
    {
        public const string Pending = nameof(Pending);
        public const string Processed = nameof(Processed);
        public const string Failed = nameof(Failed);
    }

    public InBoxMessage(string id, string pubSubName, string eventName, string topicName, string status, string payload)
        : base(id, pubSubName, eventName, topicName, status, payload, null, "IN", 0)
    {
    }
}