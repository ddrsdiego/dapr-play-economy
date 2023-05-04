namespace Play.Inventory.Service.Subscribers;

using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common.Application.Messaging;
using Common.Application.Messaging.InBox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Core.Application.Helpers.Constants;
using Core.Application.UseCases.CustomerUpdated;
using Microsoft.AspNetCore.Mvc;

public readonly struct CustomerUpdatedRequest
{
    [JsonConstructor]
    public CustomerUpdatedRequest(string customerId, string name, string email)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
    }

    public string CustomerId { get; }
    public string Name { get; }
    public string Email { get; }

    public CustomerUpdatedCommand ToCommand() => new(CustomerId, Name, Email);
}

internal static class CustomerUpdatedSubscriber
{
    public static void HandleCustomerUpdated(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"/{Topics.CustomerUpdated}", async context =>
        {
            var cancellationToken = context.RequestAborted;

            var messageEnvelopeResult = await context.ReadFromBodyAsync<MessageEnvelope>();
            if (messageEnvelopeResult.IsFailure)
                await context.Response.WriteAsync("", cancellationToken: cancellationToken);

            try
            {
                
                var inBoxMessagesRepository = context.RequestServices.GetRequiredService<IInBoxMessagesRepository>();
                await inBoxMessagesRepository.SaveAsync(messageEnvelopeResult.Value, cancellationToken);
                
                await context.Response.WriteAsJsonAsync(new OkResult(), cancellationToken: context.RequestAborted);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }).WithTopic(DaprSettings.PubSub.Name, Topics.CustomerUpdated);
    }
}