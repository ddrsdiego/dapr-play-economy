namespace Play.Inventory.Core.Domain.AggregateModel.CatalogItemAggregate
{
    public record UpdateNewCatalogItemRequest(string CatalogItemId, string Name, string Description);
}