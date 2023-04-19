namespace Play.Inventory.Core.Domain.AggregateModel.CatalogItemAggregate
{
    public record RegisterNewCatalogItemRequest(string CatalogItemId, string Name, string Description);
}