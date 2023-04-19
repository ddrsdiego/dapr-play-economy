namespace Play.Catalog.Core.Domain.AggregatesModel.CatalogItemAggregate
{
    public record CatalogItemUpdated(string CatalogItemId, string? Name, string? Description, decimal UnitPrice);
}