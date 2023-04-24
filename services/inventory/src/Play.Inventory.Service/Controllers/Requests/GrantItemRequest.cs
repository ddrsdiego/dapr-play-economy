namespace Play.Inventory.Service.Controllers.Requests
{
    using System.Text.Json.Serialization;
    using Core.Application.UseCases.GrantItem;

    public sealed class GrantItemRequest
    {
        [JsonConstructor]
        public GrantItemRequest(string userId, string catalogItemId, int quantity)
        {
            UserId = userId;
            CatalogItemId = catalogItemId;
            Quantity = quantity;
        }

        public string UserId { get; }
        public string CatalogItemId { get; }
        public int Quantity { get; }

        public GrantItemCommand ToGrantItemCommand() => new(UserId, CatalogItemId, Quantity);
    }
}