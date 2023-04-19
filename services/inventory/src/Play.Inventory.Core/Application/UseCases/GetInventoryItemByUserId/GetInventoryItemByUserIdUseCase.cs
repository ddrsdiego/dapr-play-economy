namespace Play.Inventory.Core.Application.UseCases.GetInventoryItemByUserId
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Common.Application.UseCase;
    using Infra.Repositories.CatalogItemRepository;
    using Infra.Repositories.CustomerRepository;
    using Infra.Repositories.InventoryItemRepository;
    using Microsoft.Extensions.Logging;
    using Responses;

    public sealed class GetInventoryItemByUserIdUseCase : UseCaseExecutor<GetInventoryItemByUserIdReq>
    {
        private readonly IDaprStateEntryRepository<CustomerData> _customerDaprRepository;
        private readonly IDaprStateEntryRepository<CatalogItemData> _catalogItemDaprRepository;
        private readonly IDaprStateEntryRepository<InventoryItemData> _inventoryRepository;

        public GetInventoryItemByUserIdUseCase(ILoggerFactory logger,
            IDaprStateEntryRepository<CustomerData> customerDaprRepository,
            IDaprStateEntryRepository<CatalogItemData> catalogItemDaprRepository,
            IDaprStateEntryRepository<InventoryItemData> inventoryRepository)
            : base(logger.CreateLogger<GetInventoryItemByUserIdUseCase>())
        {
            _customerDaprRepository = customerDaprRepository;
            _catalogItemDaprRepository = catalogItemDaprRepository;
            _inventoryRepository = inventoryRepository;
        }

        protected override async Task<Response> ExecuteSendAsync(GetInventoryItemByUserIdReq request,
            CancellationToken token = new CancellationToken())
        {
            var getInventoryTask = _inventoryRepository.GetByIdAsync(request.UserId, token);
            var getCustomerTask = _customerDaprRepository.GetByIdAsync(request.UserId, token);
            
            await Task.WhenAll(getCustomerTask, getInventoryTask);

            var inventoryItemResult = await getInventoryTask;
            if (inventoryItemResult.IsFailure)
                return Response.Fail(new Error("INVENTORY_ITEM_NOT_FOUND", "Inventory Item not found"));

            var customerResult = await getCustomerTask;
            var inventoryItem = inventoryItemResult.Value.ToInventoryItem(customerResult.Value);

            var catalogItemIds = inventoryItem.Items.Select(x => x.CatalogItemId).ToArray();
            var catalogItemsData = await _catalogItemDaprRepository.GetByIdAsync(catalogItemIds, token);

            var catalogItems = catalogItemsData
                .Select(x => x.Value.Value.ToStateEntry()).ToList().AsReadOnly();

            var inventoryItems = inventoryItem.ToGetInventoryItemByUserIdResponse(catalogItems);
            
            return Response.Ok(ResponseContent.Create(inventoryItems));
        }
    }
}