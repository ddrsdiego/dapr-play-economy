namespace Play.Inventory.Core.Application.Responses
{
    public class GetInventoryItemByUserIdResponse
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<GetInventoryItemLineResponse> Items { get; set; }
    }

    public sealed record GetInventoryItemLineResponse(string CatalogItemId, string Name, string Description,
        int Quantity, DateTimeOffset AcquiredAt);
}