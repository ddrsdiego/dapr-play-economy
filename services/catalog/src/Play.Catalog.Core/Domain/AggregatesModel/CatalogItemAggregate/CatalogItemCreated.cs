namespace Play.Catalog.Core.Domain.AggregatesModel.CatalogItemAggregate
{
    public record CatalogItemCreated(string CatalogItemId, string? Name, string? Description);
}