namespace Play.Inventory.Service.Subscribers;

using System;
using Common.Application.Messaging;
using Common.Application.Messaging.InBox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Core.Application.Helpers.Constants;
using Microsoft.AspNetCore.Mvc;

internal static class CustomerUpdatedSubscriber
{
    public static void HandleCustomerUpdated(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"/{Topics.CustomerUpdated}", async context =>
        {
            var cancellationToken = context.RequestAborted;

            var messageEnvelopeResult = await context.ReadFromBodyAsync<MessageEnvelope>();
            if (messageEnvelopeResult.IsFailure)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new NotFoundResult(), cancellationToken: cancellationToken);
            }

            try
            {
                var inBoxMessagesRepository = context.RequestServices.GetRequiredService<IInBoxMessagesRepository>();
                await inBoxMessagesRepository.SaveAsync(messageEnvelopeResult.Value, cancellationToken);

                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsJsonAsync(new OkResult(), cancellationToken: context.RequestAborted);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new BadRequestResult(), cancellationToken: context.RequestAborted);
            }
        }).WithTopic(DaprSettings.PubSub.Name, Topics.CustomerUpdated);
    }
}