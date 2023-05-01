namespace Play.Common.Application.Infra.Outbox;

using System;

public sealed class OutboxMessageData
{
    public string Id { get; set; }
    public string EventName { get; set; }
    public string TopicName { get; set; }
    public string FullName { get; set; }
    public string Payload { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public int NumberAttempts { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}