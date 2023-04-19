namespace Play.Catalog.Core.Domain.AggregatesModel.CatalogItemAggregate
{
    public interface ICatalogItemRepository
    {
        Task SaveOrUpdateAsync(CatalogItem entity);

        Task<CatalogItem> GetByIdAsync(string? id);
    }
}