namespace Play.Inventory.Core.Application.UseCases.CreateCatalogItem
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Common.Application.UseCase;
    using Domain.AggregateModel.CatalogItemAggregate;
    using Infra.Repositories.CatalogItemRepository;
    using Microsoft.Extensions.Logging;

    public sealed class CreateCatalogItemUseCase : UseCaseExecutor<CreateCatalogItemReq>
    {
        private readonly IDaprStateEntryRepository<CatalogItemData> _catalogItemDaprRepository;

        public CreateCatalogItemUseCase(ILoggerFactory logger,
            IDaprStateEntryRepository<CatalogItemData> catalogItemDaprRepository)
            : base(logger.CreateLogger<CreateCatalogItemUseCase>())
        {
            _catalogItemDaprRepository = catalogItemDaprRepository;
        }

        protected override async Task<Response> ExecuteSendAsync(CreateCatalogItemReq request,
            CancellationToken token = new CancellationToken())
        {
            var catalogItemDataResult = await _catalogItemDaprRepository.GetByIdAsync(request.CatalogItemId, token);
            
            var catalogItem = new CatalogItem(request.CatalogItemId, request.Name, request.Description,
                DateTimeOffset.UtcNow);
            
            var catalogItemData = catalogItem.ToCatalogItemData();
            await _catalogItemDaprRepository.UpsertAsync(catalogItemData, token);
            
            return Response.Ok();
        }
    }
}