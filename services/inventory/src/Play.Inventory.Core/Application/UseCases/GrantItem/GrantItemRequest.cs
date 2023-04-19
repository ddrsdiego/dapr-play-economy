namespace Play.Inventory.Core.Application.UseCases.GrantItem
{
    using Common.Application.UseCase;

    public sealed class GrantItemRequest : UseCaseRequest
    {
        public GrantItemRequest(string userId, string catalogItemId, int quantity)
        {
            UserId = userId;
            CatalogItemId = catalogItemId;
            Quantity = quantity;
        }

        public string UserId { get; }
        public string CatalogItemId { get; }
        public int Quantity { get; }
    }
}