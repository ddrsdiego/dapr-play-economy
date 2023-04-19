namespace Play.Inventory.Core.Application.UseCases.GetCustomerById
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Common.Application.UseCase;
    using Domain.AggregateModel.CustomerAggregate;
    using Infra.Clients;
    using Infra.Repositories.CustomerRepository;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public sealed class GetCustomerByIdUseCase : UseCaseExecutor<GetCustomerByIdRequest>
    {
        private readonly ICustomerClient _customerClient;
        private readonly IDaprStateEntryRepository<CustomerData> _customerRepository;

        public GetCustomerByIdUseCase(ILoggerFactory logger,
            IDaprStateEntryRepository<CustomerData> customerRepository, ICustomerClient customerClient)
            : base(logger.CreateLogger<GetCustomerByIdUseCase>())
        {
            _customerRepository = customerRepository;
            _customerClient = customerClient;
        }

        protected override async Task<Response> ExecuteSendAsync(GetCustomerByIdRequest request,
            CancellationToken token = new CancellationToken())
        {
            var customerResult = await _customerRepository.GetByIdAsync(request.UserId, token);
            if (customerResult.IsSuccess)
                return Response.Ok(ResponseContent.Create(customerResult.Value));

            var customerClientResult = await _customerClient.GetCustomerById(request.UserId);
            if (customerClientResult.IsFailure)
                return Response.Fail(new Error("CUSTOMER_NOT_FOUND", $"Customer no found to id: {request.UserId}"),
                    StatusCodes.Status404NotFound);

            var customerEntity = new Customer(customerClientResult.Value.CustomerId, customerClientResult.Value.Name,
                customerClientResult.Value.Email, DateTimeOffset.UtcNow);

            await _customerRepository.UpsertAsync(customerEntity.ToStateEntry(), token);
            return Response.Ok(ResponseContent.Create(customerEntity.ToStateEntry()), StatusCodes.Status201Created);
        }
    }
}