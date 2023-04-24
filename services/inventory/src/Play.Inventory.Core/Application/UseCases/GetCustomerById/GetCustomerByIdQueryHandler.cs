namespace Play.Inventory.Core.Application.UseCases.GetCustomerById
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Domain.AggregateModel.CustomerAggregate;
    using Infra.Clients;
    using Infra.Repositories.CustomerRepository;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdRequest, Response>
    {
        private readonly ICustomerClient _customerClient;
        private readonly IDaprStateEntryRepository<CustomerData> _customerRepository;

        public GetCustomerByIdQueryHandler(ICustomerClient customerClient,
            IDaprStateEntryRepository<CustomerData> customerRepository)
        {
            _customerClient = customerClient;
            _customerRepository = customerRepository;
        }

        public async Task<Response> Handle(GetCustomerByIdRequest request, CancellationToken cancellationToken)
        {
            var customerResult = await _customerRepository.GetCustomerByIdAsync(request.UserId, cancellationToken);
            if (customerResult.IsSuccess)
                return Response.Ok(ResponseContent.Create(customerResult.Value));

            var customerClientResult = await _customerClient.GetCustomerByIdAsync(request.UserId);
            if (customerClientResult.IsFailure)
                return Response.Fail(new Error("CUSTOMER_NOT_FOUND", $"Customer no found to id: {request.UserId}"),
                    StatusCodes.Status404NotFound);

            var customerEntity = new Customer(customerClientResult.Value.CustomerId, customerClientResult.Value.Name,
                customerClientResult.Value.Email, DateTimeOffset.UtcNow);

            var customerStateEntry = customerEntity.ToStateEntry();
            await _customerRepository.UpsertAsync(customerStateEntry, cancellationToken);
            
            return Response.Ok(ResponseContent.Create(customerStateEntry), StatusCodes.Status201Created);
        }
    }
}