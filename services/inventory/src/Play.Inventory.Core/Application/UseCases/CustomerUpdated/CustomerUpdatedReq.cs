namespace Play.Inventory.Core.Application.UseCases.CustomerUpdated
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Common.Application.UseCase;
    using Domain.AggregateModel.CustomerAggregate;
    using Infra.Repositories.CustomerRepository;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class CustomerUpdatedReq : UseCaseRequest
    {
        public CustomerUpdatedReq(string customerId, string name, string email)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
        }

        public string CustomerId { get; }
        public string Name { get; }
        public string Email { get; }
    }

    internal sealed class CustomerUpdatedUseCase : UseCaseExecutor<CustomerUpdatedReq>
    {
        private readonly IDaprStateEntryRepository<CustomerData> _customerDaprRepository;

        public CustomerUpdatedUseCase(ILoggerFactory logger, IDaprStateEntryRepository<CustomerData> customerDaprRepository)
            : base(logger.CreateLogger<CustomerUpdatedUseCase>())
        {
            _customerDaprRepository = customerDaprRepository;
        }

        protected override async Task<Response> ExecuteSendAsync(CustomerUpdatedReq request,
            CancellationToken token = default)
        {
            var customerResult = await _customerDaprRepository.GetByIdAsync(request.CustomerId, token);
            if (customerResult.IsFailure)
                Response.Ok();
            
            var newCustomer = new Customer(request.CustomerId, request.Name,
                request.Email);
                
            await _customerDaprRepository.UpsertAsync(newCustomer.ToStateEntry(), token);
            return Response.Ok(StatusCodes.Status204NoContent);
        }
    }
}