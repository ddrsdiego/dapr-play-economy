namespace Play.Inventory.Core.Application.Infra.Repositories.InventoryItemRepository
{
    using Play.Common.Application.Infra.Repositories.Dapr;

    [StateEntryName("inventory-item")]
    public class InventoryItemData : IDaprStateEntry
    {
        public InventoryItemData(string stateEntryKey) => StateEntryKey = stateEntryKey;

        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public IEnumerable<InventoryItemLineStateEntry> Items { get; init; }
        public string StateEntryKey { get; }
    }

    public class InventoryItemLineStateEntry
    {
        public string CatalogItemId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset AcquiredAt { get; set; }
    }
}