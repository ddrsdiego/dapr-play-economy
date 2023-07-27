namespace Play.Common.Application.Messaging.InBox;

using System;

public sealed class DeserializeInBoxMessagePayloadException : Exception
{
    public DeserializeInBoxMessagePayloadException(InBoxMessage inBoxMessage, string errorMessage)
        : base(CreatMessage(inBoxMessage, errorMessage))
    {
    }

    private static string CreatMessage(InBoxMessage inBoxMessage, string errorMessage) =>
        $"Error on deserialize payload from event type {inBoxMessage.EventName} for FullName {inBoxMessage.FullName} /n Error: {errorMessage}";
}