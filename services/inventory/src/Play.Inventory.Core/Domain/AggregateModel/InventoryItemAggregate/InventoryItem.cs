namespace Play.Inventory.Core.Domain.AggregateModel.InventoryItemAggregate
{
    using System.Collections.Immutable;
    using System.Runtime.CompilerServices;
    using CustomerAggregate;

    public sealed class InventoryItem
    {
        private ImmutableDictionary<string, InventoryItemLine> _items;

        private InventoryItem()
            : this(Customer.Default, default)
        {
        }

        public InventoryItem(Customer customer)
            : this(customer, DateTimeOffset.UtcNow)
        {
        }

        internal InventoryItem(Customer customer, DateTimeOffset createdAt)
        {
            Customer = customer;
            CreatedAt = createdAt;
            _items = ImmutableDictionary<string, InventoryItemLine>.Empty;
        }

        public static InventoryItem Default => new();

        public Customer Customer { get; }
        
        public DateTimeOffset CreatedAt { get; }

        public IReadOnlyCollection<InventoryItemLine> Items => _items.Values.ToList().AsReadOnly();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNewItemLine(InventoryItemLine inventoryItemLine)
        {
            if (_items.TryGetValue(inventoryItemLine.CatalogItemId, out var item))
            {
                _items = _items.Remove(inventoryItemLine.CatalogItemId);

                item.UpdateQuantity(inventoryItemLine.Quantity);
                _items = _items.Add(inventoryItemLine.CatalogItemId, item);
            }
            else
            {
                _items = _items.Add(inventoryItemLine.CatalogItemId, inventoryItemLine);
            }
        }
    }
}