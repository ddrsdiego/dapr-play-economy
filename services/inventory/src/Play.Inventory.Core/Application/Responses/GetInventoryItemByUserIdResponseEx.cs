namespace Play.Inventory.Core.Application.Responses
{
    using System.Runtime.CompilerServices;
    using Domain.AggregateModel.CatalogItemAggregate;
    using Domain.AggregateModel.InventoryItemAggregate;

    public static class GetInventoryItemByUserIdResponseEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GetInventoryItemByUserIdResponse ToGetInventoryItemByUserIdResponse(
            this InventoryItem line, IReadOnlyCollection<CatalogItem> catalogItems)
        {
            var items = line.Items.Select(item =>
            {
                var catalogItem = catalogItems.SingleOrDefault(x => x.CatalogItemId == item.CatalogItemId);
                return new GetInventoryItemLineResponse(item.CatalogItemId, catalogItem.Name, catalogItem.Description,
                    item.Quantity,
                    item.AcquiredAt);
            });

            return new GetInventoryItemByUserIdResponse
            {
                Name = line.Customer.Name,
                Email = line.Customer.Email,
                Items = items
            };
        }
    }
}