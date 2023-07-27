namespace Play.Common.Application.Messaging.InBox.Extensions;

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using CSharpFunctionalExtensions;
using Dapper;

internal static class InBoxMessageEx
{
    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TEvent> TryParse<TEvent>(this InBoxMessage inBoxMessage)
    {
        try
        {
            if (!TypeCache.TryGetValue(inBoxMessage.FullName, out var type))
            {
                type = Type.GetType(inBoxMessage.FullName);
                if (type == null)
                    return Result.Failure<TEvent>($"The type {inBoxMessage.FullName} could not be found");

                TypeCache[inBoxMessage.FullName] = type;
            }
            
            var deserializedPayload = Serializer.FromJson<TEvent>(inBoxMessage.Payload, type);
            return Result.Success(deserializedPayload);
        }
        catch (Exception e)
        {
            return Result.Failure<TEvent>($"The type {inBoxMessage.FullName} could not be found. Error: {e.Message}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static InBoxMessage ToInBoxMessageData(this InBoxMessageData inBoxMessageData, string processorId)
    {
        var priorityResult = InBoxMessagePriority.GetByValue(inBoxMessageData.Priority);

        var inBoxMessage = new InBoxMessage
        {
            MessageId = inBoxMessageData.MessageId,
            ProcessorId = processorId,
            CorrelationId = inBoxMessageData.CorrelationId,
            EventName = inBoxMessageData.EventName,
            TopicName = inBoxMessageData.TopicName,
            PubSubName = inBoxMessageData.PubSubName,
            Payload = inBoxMessageData.Payload,
            FullName = inBoxMessageData.FullName,
            Status = inBoxMessageData.Status,
            NumberAttempts = inBoxMessageData.NumberAttempts,
            Priority = priorityResult.Value.Value,
            ReceiveEndpoint = inBoxMessageData.ReceiveEndpoint
        };

        return inBoxMessage;
    }

    public static DynamicParameters ToSaveParameters(this InBoxMessage[] messages)
    {
        var parameters = new DynamicParameters();
        for (var intCounter = 0; intCounter < messages.Length; intCounter++)
        {
            parameters.Add($"@MessageId{intCounter}", messages[intCounter].MessageId);
            parameters.Add($"@CorrelationId{intCounter}", messages[intCounter].CorrelationId);
            parameters.Add($"@PubSubName{intCounter}", messages[intCounter].PubSubName);
            parameters.Add($"@EventName{intCounter}", messages[intCounter].EventName);
            parameters.Add($"@TopicName{intCounter}", messages[intCounter].TopicName);
            parameters.Add($"@Payload{intCounter}", messages[intCounter].Payload);
            parameters.Add($"@Status{intCounter}", messages[intCounter].Status);
            parameters.Add($"@FullName{intCounter}", messages[intCounter].FullName);
            parameters.Add($"@ReceiveEndpoint{intCounter}", messages[intCounter].ReceiveEndpoint);
            parameters.Add($"@Priority{intCounter}", messages[intCounter].Priority);
        }

        return parameters;
    }

    public static string ToSaveQuery(this InBoxMessage[] messages)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine(@"INSERT INTO public.""InBoxMessage"" (""MessageId"", ""CorrelationId"", ""PubSubName"", ""EventName"", ""TopicName"", ""Payload"", ""Status"", ""FullName"", ""ReceiveEndpoint"", ""Priority"")");
        queryBuilder.AppendLine("VALUES ");

        for (var intCounter = 0; intCounter < messages.Length; intCounter++)
        {
            queryBuilder.AppendLine("(");
            queryBuilder.AppendLine($"@MessageId{intCounter}");
            queryBuilder.AppendLine($",@CorrelationId{intCounter}");
            queryBuilder.AppendLine($",@PubSubName{intCounter}");
            queryBuilder.AppendLine($",@EventName{intCounter}");
            queryBuilder.AppendLine($",@TopicName{intCounter}");
            queryBuilder.AppendLine($",@Payload{intCounter}");
            queryBuilder.AppendLine($",@Status{intCounter}");
            queryBuilder.AppendLine($",@FullName{intCounter}");
            queryBuilder.AppendLine($",@ReceiveEndpoint{intCounter}");
            queryBuilder.AppendLine($",@Priority{intCounter}");
            queryBuilder.AppendLine(")");

            if (intCounter < messages.Length - 1) queryBuilder.AppendLine(",");
        }

        return queryBuilder.ToString();
    }

    public static string ToRegisterFollowUpQuery(this InBoxMessage[] messages)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine(@"INSERT INTO public.""InBoxMessageFollowUp"" (""InBoxMessageFollowUpId"", ""MessageId"", ""Status"", ""UpdateAt"", ""Exception"")");
        queryBuilder.AppendLine("VALUES");

        for (var intCounter = 0; intCounter < messages.Length; intCounter++)
        {
            queryBuilder.AppendLine("(");
            queryBuilder.AppendLine($"@InBoxMessageFollowUpId{intCounter},");
            queryBuilder.AppendLine($"@MessageId{intCounter},");
            queryBuilder.AppendLine($"@Status{intCounter},");
            queryBuilder.AppendLine($"@UpdateAt{intCounter},");
            queryBuilder.AppendLine($"@Exception{intCounter}");
            queryBuilder.AppendLine(")");

            if (intCounter < messages.Length - 1) queryBuilder.AppendLine(",");
        }

        return queryBuilder.ToString();
    }

    public static DynamicParameters ToRegisterFollowUpParameters(this InBoxMessage[] messages, string status, string errorMessage = "")
    {
        var parameters = new DynamicParameters();
        for (var intCounter = 0; intCounter < messages.Length; intCounter++)
        {
            parameters.Add($"@InBoxMessageFollowUpId{intCounter}", Guid.NewGuid().ToString());
            parameters.Add($"@MessageId{intCounter}", messages[intCounter].MessageId);
            parameters.Add($"@Status{intCounter}", status);
            parameters.Add($"@UpdateAt{intCounter}", DateTime.Now);
            parameters.Add($"@Exception{intCounter}", string.IsNullOrEmpty(errorMessage) ? null : errorMessage);
        }

        return parameters;
    }
}