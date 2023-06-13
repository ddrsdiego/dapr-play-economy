namespace Play.Common.Application.Messaging.InBox;

using System;
using Messaging;

public sealed class InBoxMessage : BoxMessage
{
    public string Receiver { get; }

    public string ProcessorId { get; }

    public static class InBoxMessageStatus
    {
        public const string Pending = nameof(Pending);
        public const string Processed = nameof(Processed);
        public const string Failed = nameof(Failed);
    }

    public const string BoxType = "IN";

    public InBoxMessage(string messageId, string processorId, string pubSubName, string eventName, string topicName, string payload, string receiver, string fullName)
        : base(messageId, pubSubName, eventName, topicName,  payload, fullName)
    {
        Receiver = receiver;
        ProcessorId = processorId;
    }

    public class InBoxMessageReceiver
    {
        public InBoxMessageReceiver(string receiver, string machineReceiver, DateTimeOffset receivedAt)
        {
        }
    }
}