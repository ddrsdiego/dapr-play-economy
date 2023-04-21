namespace Play.Catalog.Core.Application.UseCases.UpdateUnitPriceCatalogItem
{
    using Common.Application;
    using Common.Application.UseCase;
    using MediatR;

    public sealed class UpdateUnitPriceCatalogItemRequest : UseCaseRequest, IRequest<Response>
    {
        public UpdateUnitPriceCatalogItemRequest(string catalogItemId, decimal unitPrice)
            : this(Guid.NewGuid().ToString(), catalogItemId, unitPrice)
        {
        }

        public UpdateUnitPriceCatalogItemRequest(string requestId, string catalogItemId, decimal unitPrice)
            : base(requestId)
        {
            UnitPrice = unitPrice;
            CatalogItemId = catalogItemId;
        }
        
        public string CatalogItemId { get; }
        public decimal UnitPrice { get; }
    }
}