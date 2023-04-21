namespace Play.Inventory.Core.Application.UseCases.CreateCatalogItem
{
    using Common.Application;
    using Common.Application.UseCase;
    using MediatR;

    public sealed class CreateCatalogItemReq : UseCaseRequest, IRequest<Response>
    {
        public CreateCatalogItemReq(string catalogItemId, string name, string description, decimal unitPrice)
        {
            CatalogItemId = catalogItemId;
            Name = name;
            Description = description;
            UnitPrice = unitPrice;
        }

        public string CatalogItemId { get; }
        public string Name { get; }
        public string Description { get; }
        public decimal UnitPrice { get; }
    }
}