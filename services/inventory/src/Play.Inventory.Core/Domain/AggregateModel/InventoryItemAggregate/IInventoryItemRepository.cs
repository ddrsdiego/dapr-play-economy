namespace Play.Inventory.Core.Domain.AggregateModel.InventoryItemAggregate
{
    public interface IInventoryItemRepository
    {
        Task SaveOrUpdateAsync(InventoryItem inventoryItem);
        
        Task<InventoryItem> GetByUserIdAsync(string userId);
    }
}