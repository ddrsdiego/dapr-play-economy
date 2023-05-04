namespace Play.Catalog.Core.Application.UseCases.UpdateUnitPriceCatalogItem;

using Common.Application;
using MediatR;

public record struct UpdateUnitPriceCatalogItemCommand(string CatalogItemId, decimal UnitPrice) : IRequest<Response>;

public sealed class UpdateUnitPriceCatalogItemRequest
{
    public decimal UnitPrice { get; }
}