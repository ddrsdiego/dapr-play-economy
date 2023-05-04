namespace Play.Inventory.Service.Subscribers;

using System.Text.Json.Serialization;
using Common.Application;
using Common.Application.Infra.Repositories.Dapr;
using Core.Application.Helpers.Constants;
using Core.Application.Infra.Repositories.CatalogItemRepository;
using Core.Domain.AggregateModel.CatalogItemAggregate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public readonly struct CatalogItemCreated
{
    [JsonConstructor]
    public CatalogItemCreated(string catalogItemId, string name, string description, decimal unitPrice)
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

internal static class CatalogItemCreatedSubscriber
{
    public static void HandleCatalogItemCreated(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"/{Topics.CatalogItemCreated}", async context =>
        {
            var catalogItemCreated = await context.ReadFromBodyAsync<CatalogItemCreated>();
            if (catalogItemCreated.IsFailure)
            {
                var error = new Error("", "");
                var response = Response.Fail(error);
                await response.WriteToPipeAsync(context.Response);
            }
            
            var catalogItemDaprRepository =
                context.RequestServices.GetRequiredService<IDaprStateEntryRepository<CatalogItemData>>();

            var newCatalogItem = new CatalogItem(catalogItemCreated.Value.CatalogItemId,
                catalogItemCreated.Value.Name,
                catalogItemCreated.Value.Description);

            await catalogItemDaprRepository.UpsertAsync(newCatalogItem.ToCatalogItemData());
            await context.Response.WriteAsJsonAsync(new OkResult(), cancellationToken: context.RequestAborted);
        }).WithTopic(DaprSettings.PubSub.Name, Topics.CatalogItemCreated);
    }
}