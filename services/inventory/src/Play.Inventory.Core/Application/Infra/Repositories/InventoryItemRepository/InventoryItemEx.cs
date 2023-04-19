namespace Play.Inventory.Core.Application.Infra.Repositories.InventoryItemRepository
{
    using System.Runtime.CompilerServices;
    using CustomerRepository;
    using Domain.AggregateModel.InventoryItemAggregate;

    public static class InventoryItemEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InventoryItemData ToStateEntry(this InventoryItem inventoryItemLine)
        {
            var itemsData = inventoryItemLine.Items
                .Select(item => new InventoryItemLineStateEntry
                {
                    CatalogItemId = item.CatalogItemId,
                    Quantity = item.Quantity,
                    AcquiredAt = item.AcquiredAt,
                });

            return new InventoryItemData(inventoryItemLine.Customer.CustomerId)
            {
                Id = inventoryItemLine.Customer.CustomerId,
                UserId = inventoryItemLine.Customer.CustomerId,
                Items = itemsData,
                CreatedAt = inventoryItemLine.CreatedAt
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InventoryItem ToInventoryItem(this InventoryItemData inventoryItemData, CustomerData customerData)
        {
            var customer = customerData.ToCustomer();
            var inventoryItem = new InventoryItem(customer, inventoryItemData.CreatedAt);

            foreach (var item in inventoryItemData.Items)
            {
                inventoryItem.AddNewItemLine(new InventoryItemLine(item.CatalogItemId, item.Quantity, item.AcquiredAt));
            }

            return inventoryItem;
        }
    }
}