namespace Play.Inventory.Core.Application.UseCases.GrantItem
{
    using Common.Application;
    using MediatR;

    public readonly struct GrantItemCommand : IRequest<Response>
    {
        public GrantItemCommand(string userId, string catalogItemId, int quantity)
        {
            UserId = userId;
            CatalogItemId = catalogItemId;
            Quantity = quantity;
        }

        public readonly string UserId;
        public readonly string CatalogItemId;
        public readonly int Quantity;
    }
}