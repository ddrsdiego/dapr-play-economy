namespace Play.Inventory.Service.Subscribers;

using System;
using System.Text.Json.Serialization;
using Common.Application.Messaging;
using Common.Application.Messaging.InBox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Core.Application.Helpers.Constants;
using Core.Application.UseCases.CreateCatalogItem;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public readonly struct CatalogItemUpdated
{
    [JsonConstructor]
    public CatalogItemUpdated(string catalogItemId, string name, string description, decimal unitPrice)
    {
        CatalogItemId = catalogItemId;
        Name = name;
        Description = description;
        UnitPrice = unitPrice;
    }

    public string CatalogItemId { get; }
    public string Name { get; }
    public string Description { get; }
    public decimal UnitPrice { get; }
}

internal static class CatalogItemUpdatedSubscriber
{
    public static void HandleCatalogItemUpdated(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"/{Topics.CatalogItemUpdated}", async context =>
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
        }).WithTopic(DaprSettings.PubSub.Name, Topics.CatalogItemUpdated);
    }
}

internal static class CreateCatalogItemReqEx
{
    public static CreateCatalogItemCommand ToCommand(this CatalogItemUpdated catalogItemUpdated) =>
        new(catalogItemUpdated.CatalogItemId,
            catalogItemUpdated.Name,
            catalogItemUpdated.Description,
            catalogItemUpdated.UnitPrice);
}