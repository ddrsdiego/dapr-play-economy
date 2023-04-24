namespace Play.Inventory.Core.Application.UseCases.CustomerUpdated
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Domain.AggregateModel.CustomerAggregate;
    using Infra.Repositories.CustomerRepository;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    internal sealed class CustomerUpdatedCommandHandler : IRequestHandler<CustomerUpdatedCommand,Response>
    {
        private readonly IDaprStateEntryRepository<CustomerData> _customerDaprRepository;

        public CustomerUpdatedCommandHandler(IDaprStateEntryRepository<CustomerData> customerDaprRepository)
        {
            _customerDaprRepository = customerDaprRepository;
        }

        public async Task<Response> Handle(CustomerUpdatedCommand request, CancellationToken cancellationToken)
        {
            var customerResult = await _customerDaprRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken);
            if (customerResult.IsFailure)
                Response.Ok();
            
            var newCustomer = new Customer(request.CustomerId, request.Name,
                request.Email);
                
            await _customerDaprRepository.UpsertAsync(newCustomer.ToStateEntry(), cancellationToken);
            return Response.Ok(StatusCodes.Status204NoContent);
        }
    }
}