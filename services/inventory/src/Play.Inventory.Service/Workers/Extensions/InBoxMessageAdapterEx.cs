namespace Play.Inventory.Service.Workers.Extensions;

using System;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Play.Common.Application.Messaging.InBox;

public static class InBoxMessageAdapterEx
{
    public static Result<TEvent> AdapterFromInBoxMessage<TEvent>(this InBoxMessage inBoxMessage)
    {
        var type = Type.GetType(inBoxMessage.FullName);
        var eventToProcess = (TEvent) JsonSerializer.Deserialize(inBoxMessage.Payload, type, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Result.Success(eventToProcess);
    }
}