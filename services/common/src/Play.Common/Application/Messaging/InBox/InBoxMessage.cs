namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Diagnostics;

public sealed class InBoxMessage
{
    public static class InBoxMessageStatus
    {
        public const string Pending = nameof(Pending);
        public const string InProgress = nameof(InProgress);
        public const string Completed = nameof(Completed);
        public const string Failed = nameof(InProgress);
    }

    private const int InitialNumberAttempts = 0;
    private readonly Stopwatch _processorExecuteWatch = new();

    internal InBoxMessage()
    {
        Status = InBoxMessageStatus.Pending;
        NumberAttempts = InitialNumberAttempts;
    }

    public static InBoxMessage CreateMessagePending<TMessage>(string correlationId, string topicName, string pubSubName, string receiveEndpoint, TMessage message) =>
        CreateMessagePending(correlationId, topicName, pubSubName, receiveEndpoint, InBoxMessagePriority.None, message);

    public static InBoxMessage CreateMessagePending<TMessage>(string correlationId, string topicName, string pubSubName, string receiveEndpoint, InBoxMessagePriority priority, TMessage message)
    {
        var eventName = message.GetType().Name;
        var fullName = message.GetType().FullName;
        var payload = Serializer.ToJson(ref message);

        var inBoxMessage = new InBoxMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            EventName = eventName,
            TopicName = topicName,
            PubSubName = pubSubName,
            Payload = payload,
            FullName = fullName,
            Status = InBoxMessageStatus.Pending,
            NumberAttempts = InitialNumberAttempts,
            Priority = priority.Value,
            ReceiveEndpoint = receiveEndpoint
        };

        return inBoxMessage;
    }

    internal void UpdateStatus(string status) => Status = status;

    public string MessageId { get; internal set; }
    public string CorrelationId { get; internal set; }
    public string EventName { get; internal set; }
    public string Payload { get; internal set; }
    public string PubSubName { get; internal set; }
    public string Status { get; internal set; }
    public string FullName { get; internal set; }
    public string TopicName { get; internal set; }
    public int NumberAttempts { get; internal set; }
    public int Priority { get; internal set; }
    public string ReceiveEndpoint { get; set; }
    public string ProcessorId { get; internal set; }
    
    public void IncrementNumberAttempts() => NumberAttempts++;

    internal void StartProcessorExecuteWatch() => _processorExecuteWatch.Start();
    internal void StopProcessorExecuteWatch() => _processorExecuteWatch.Stop();

    internal long ElapsedMilliseconds => _processorExecuteWatch.ElapsedMilliseconds;
}