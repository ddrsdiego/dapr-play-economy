namespace Play.Common.Application.Messaging.OutBox;

using System;

public struct OutBoxMessageData
{
    public string Id { get; set; }
    public string ProcessorId { get; set; }
    public string EventName { get; set; }
    public string TopicName { get; set; }
    public string FullName { get; set; }
    public string Payload { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public int NumberAttempts { get; set; }
    public string Sender { get; set; }
    public string PubSubName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}