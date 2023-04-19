namespace Play.Inventory.Service.Subscribers
{
    using System.Text.Json.Serialization;
    using Common.Application;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Common.Application.UseCase;
    using Core.Application.Helpers.Constants;
    using Core.Application.UseCases.CreateCatalogItem;
    using Microsoft.AspNetCore.Http;

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
                var readResult = await context.ReadFromBodyAsync<CatalogItemUpdated>();
                if (readResult.IsFailure)
                    await context.Response.WriteAsync("", cancellationToken: context.RequestAborted);

                var useCase = context.RequestServices.GetRequiredService<IUseCaseExecutor<CreateCatalogItemReq>>();

                var response = await useCase.SendAsync(new CreateCatalogItemReq(
                    readResult.Value.CatalogItemId,
                    readResult.Value.Name,
                    readResult.Value.Description,
                    readResult.Value.UnitPrice), context.RequestAborted);
                
                await response.WriteToPipeAsync(context.Response, context.RequestAborted);
            }).WithTopic(DaprSettings.PubSub.Name, Topics.CatalogItemUpdated);
        }
    }
}