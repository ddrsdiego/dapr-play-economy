namespace Play.Inventory.Core.Domain.AggregateModel.InventoryItemAggregate
{
    public sealed class InventoryItemLine
    {
        public InventoryItemLine(string catalogItemId, int quantity)
            : this(catalogItemId, quantity, DateTimeOffset.UtcNow)
        {
        }

        internal InventoryItemLine(string catalogItemId, int quantity, DateTimeOffset acquiredAt)
        {
            CatalogItemId = catalogItemId;
            Quantity = quantity;
            AcquiredAt = acquiredAt;
        }

        public string CatalogItemId { get; }
        public int Quantity { get; private set; }
        public DateTimeOffset AcquiredAt { get; }

        public void UpdateQuantity(int quantity)
        {
            Quantity += quantity;
        }
    }
}