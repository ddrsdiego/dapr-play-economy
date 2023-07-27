namespace Play.Common.Application.Messaging.InBox;

public struct InBoxMessageData
{
    public string MessageId { get; set; }
    public string CorrelationId { get; set; }
    public string PubSubName { get; set; }
    public string EventName { get; set; }
    public string TopicName { get; set; }
    public string Status { get; set; }
    public string Payload { get; set; }
    public string FullName { get; set; }
    public int Priority { get; set; }
    public int NumberAttempts { get; set; }
    public string ReceiveEndpoint { get; set; }
}