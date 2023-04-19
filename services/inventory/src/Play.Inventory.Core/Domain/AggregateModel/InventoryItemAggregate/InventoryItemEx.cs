// namespace Play.Inventory.Core.Domain.AggregateModel.InventoryItemAggregate
// {
//     using Application.Requests;
//
//     internal static class InventoryItemEx
//     {
//         public static InventoryItemResponse ToInventoryItemResponse(this InventoryItemLine inventoryItemLine)
//         {
//             return new InventoryItemResponse(inventoryItemLine.CatalogItemId, inventoryItemLine.Quantity,
//                 inventoryItemLine.AcquiredAt);
//         }
//     }
// }