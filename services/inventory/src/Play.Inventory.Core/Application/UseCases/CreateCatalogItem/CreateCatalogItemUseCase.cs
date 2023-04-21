namespace Play.Inventory.Core.Application.UseCases.CreateCatalogItem
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Domain.AggregateModel.CatalogItemAggregate;
    using Infra.Repositories.CatalogItemRepository;
    using MediatR;

    public sealed class CreateCatalogItemCommandHandler : IRequestHandler<CreateCatalogItemReq, Response>
    {
        private readonly IDaprStateEntryRepository<CatalogItemData> _catalogItemDaprRepository;
        
        public CreateCatalogItemCommandHandler(IDaprStateEntryRepository<CatalogItemData> catalogItemDaprRepository)
        {
            _catalogItemDaprRepository = catalogItemDaprRepository;
        }
        
        public async Task<Response> Handle(CreateCatalogItemReq request, CancellationToken cancellationToken)
        {
            var catalogItemDataResult = await _catalogItemDaprRepository.GetByIdAsync(request.CatalogItemId, cancellationToken);
            
            var catalogItem = new CatalogItem(request.CatalogItemId, request.Name, request.Description,
                DateTimeOffset.UtcNow);

            var catalogItemData = catalogItem.ToCatalogItemData();
            await _catalogItemDaprRepository.UpsertAsync(catalogItemData, cancellationToken);

            return Response.Ok();
        }
    }
}