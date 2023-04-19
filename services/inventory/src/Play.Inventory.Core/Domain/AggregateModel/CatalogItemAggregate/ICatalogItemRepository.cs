namespace Play.Inventory.Core.Domain.AggregateModel.CatalogItemAggregate
{
    public interface ICatalogItemRepository
    {
        Task<CatalogItem> GetByIdAsync(string catalogItemId);
        
        Task<IReadOnlyCollection<CatalogItem>> GetByIdsAsync(string[] catalogItemIds);
        
        Task UpsertAsync(CatalogItem newCatalogItem);
    }
}