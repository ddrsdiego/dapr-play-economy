namespace Play.Inventory.Core.Application.UseCases.GrantItem
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Domain.AggregateModel.InventoryItemAggregate;
    using GetCustomerById;
    using Infra.Repositories.CustomerRepository;
    using Infra.Repositories.InventoryItemRepository;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public sealed class GrantItemCommandHandler : IRequestHandler<GrantItemCommand, Response>
    {
        private readonly IMediator _mediator;
        private readonly IDaprStateEntryRepository<InventoryItemData> _inventoryRepository;

        public GrantItemCommandHandler(IMediator mediator,
            IDaprStateEntryRepository<InventoryItemData> inventoryRepository)
        {
            _mediator = mediator;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<Response> Handle(GrantItemCommand request, CancellationToken cancellationToken)
        {
            var customerResponse = await _mediator.Send(new GetCustomerByIdQuery(request.UserId), cancellationToken);
            if (customerResponse.IsFailure)
                return Response.Fail(new Error("CUSTOMER_NOT_FOUND", "Customer not found"));

            var customerStateEntry = customerResponse.Content.GetRaw<CustomerData>();
            var inventoryResult =
                await _inventoryRepository.GetCustomerByIdAsync(customerStateEntry.CustomerId, cancellationToken);

            var inventoryItem = inventoryResult.IsFailure
                ? new InventoryItem(customerStateEntry.ToCustomer())
                : inventoryResult.Value.ToInventoryItem(customerStateEntry);

            inventoryItem.AddNewItemLine(new InventoryItemLine(request.CatalogItemId, request.Quantity));
            var inventoryItemData = inventoryItem.ToStateEntry();

            await _inventoryRepository.UpsertAsync(inventoryItemData, cancellationToken);

            return Response.Ok(StatusCodes.Status201Created);
        }
    }
}