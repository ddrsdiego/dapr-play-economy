namespace Play.Inventory.Core.Application.UseCases.GrantItem
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Common.Application.UseCase;
    using Domain.AggregateModel.InventoryItemAggregate;
    using GetCustomerById;
    using Infra.Repositories.CustomerRepository;
    using Infra.Repositories.InventoryItemRepository;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public sealed class GrantItemUseCase : UseCaseExecutor<GrantItemRequest>
    {
        private readonly IMediator _mediator;
        private readonly IDaprStateEntryRepository<InventoryItemData> _inventoryRepository;

        public GrantItemUseCase(ILoggerFactory logger, IMediator mediator,
            IDaprStateEntryRepository<InventoryItemData> inventoryRepository)
            : base(logger.CreateLogger<GrantItemUseCase>())
        {
            _mediator = mediator;
            _inventoryRepository = inventoryRepository;
        }

        protected override async Task<Response> ExecuteSendAsync(GrantItemRequest request,
            CancellationToken token = new())
        {
            var customerResponse = await _mediator.Send(new GetCustomerByIdRequest(request.UserId), token);
            if (customerResponse.IsFailure)
                return Response.Fail(new Error("CUSTOMER_NOT_FOUND", "Customer not found"));
            
            var customerStateEntry = customerResponse.Content.GetRaw<CustomerData>();
            var inventoryResult = await _inventoryRepository.GetCustomerByIdAsync(customerStateEntry.CustomerId, token);

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