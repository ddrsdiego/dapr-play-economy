namespace Play.Common.Messaging;

using System;

public sealed class MessageEnvelope
{
    public MessageEnvelope(string sender, byte[] body)
    {
        Id = Guid.NewGuid().ToString();
        SentAt = DateTimeOffset.Now;
        Sender = sender;
        Body = body;
    }

    public byte[] Body { get; set; }
    public string Sender { get; set; }
    public string Id { get; }
    public DateTimeOffset SentAt { get; }
}