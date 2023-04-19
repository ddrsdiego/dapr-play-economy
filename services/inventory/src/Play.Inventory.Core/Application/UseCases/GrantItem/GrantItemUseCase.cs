namespace Play.Inventory.Core.Application.UseCases.GrantItem
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Common.Application.UseCase;
    using Domain.AggregateModel.InventoryItemAggregate;
    using GetCustomerById;
    using Infra.Repositories.CustomerRepository;
    using Infra.Repositories.InventoryItemRepository;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public sealed class GrantItemUseCase : UseCaseExecutor<GrantItemRequest>
    {
        private readonly IUseCaseExecutor<GetCustomerByIdRequest> _getCustomerByIdUseCase;
        private readonly IDaprStateEntryRepository<InventoryItemData> _inventoryRepository;

        public GrantItemUseCase(ILoggerFactory logger,
            IDaprStateEntryRepository<InventoryItemData> inventoryRepository,
            IUseCaseExecutor<GetCustomerByIdRequest> getCustomerByIdUseCase)
            : base(logger.CreateLogger<GrantItemUseCase>())
        {
            _inventoryRepository = inventoryRepository;
            _getCustomerByIdUseCase = getCustomerByIdUseCase;
        }

        protected override async Task<Response> ExecuteSendAsync(GrantItemRequest request,
            CancellationToken token = new())
        {
            var customerResponse = await _getCustomerByIdUseCase.SendAsync(new GetCustomerByIdRequest(request.UserId), token);
            if (customerResponse.IsFailure)
                return Response.Fail(new Error("CUSTOMER_NOT_FOUND", "Customer not found"));
            
            var customerStateEntry = customerResponse.Content.GetRaw<CustomerData>();
            var inventoryResult = await _inventoryRepository.GetByIdAsync(customerStateEntry.CustomerId, token);

            var inventoryItem = inventoryResult.IsFailure ? 
                new InventoryItem(customerStateEntry.ToCustomer()) 
                : inventoryResult.Value.ToInventoryItem(customerStateEntry);

            inventoryItem.AddNewItemLine(new InventoryItemLine(request.CatalogItemId, request.Quantity));
            var inventoryItemData = inventoryItem.ToStateEntry();
            
            await _inventoryRepository.UpsertAsync(inventoryItemData, token);

            return Response.Ok(StatusCodes.Status201Created);
        }
    }
}