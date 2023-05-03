namespace Play.Common.Messaging;

using System;
using System.Text.Json.Serialization;

public sealed class MessageEnvelope
{
    [JsonConstructor]
    public MessageEnvelope(string eventName, string topicName, string sender, byte[] body)
    {
        EnvelopeId = Guid.NewGuid().ToString();
        EventName = eventName;
        TopicName = topicName;
        Sender = sender;
        Body = body;
        SentAt = DateTimeOffset.UtcNow;
    }

    public byte[] Body { get; set; }
    public string EventName { get; }
    public string TopicName { get; }
    public string Sender { get; set; }
    public string EnvelopeId { get; }
    public DateTimeOffset SentAt { get; }
}