namespace Play.Common.Application.Messaging;

using System;
using System.Text.Json.Serialization;

public sealed class MessageEnvelope
{
    [JsonConstructor]
    public MessageEnvelope(string envelopeId,string pubSubName, string eventName, string topicName, string sender, byte[] body)
    {
        EventName = eventName;
        TopicName = topicName;
        Sender = sender;
        Body = body;
        EnvelopeId = envelopeId;
        PubSubName = pubSubName;
        SentAt = DateTimeOffset.Now;
    }

    public byte[] Body { get; set; }
    public string EventName { get; }
    public string TopicName { get; }
    public string Sender { get; set; }
    public string EnvelopeId { get; }
    public DateTimeOffset SentAt { get; }
    public string PubSubName { get; set; }
}