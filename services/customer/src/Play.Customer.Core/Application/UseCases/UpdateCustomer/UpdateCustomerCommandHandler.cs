namespace Play.Customer.Core.Application.UseCases.UpdateCustomer
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Application;
    using Dapr.Client;
    using Domain.AggregateModel.CustomerAggregate;
    using Helpers.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    internal sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerRequest, Response>
    {
        private readonly DaprClient _daprClient;
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerCommandHandler(ILogger<UpdateCustomerCommandHandler> logger,
            ICustomerRepository customerRepository, DaprClient daprClient)
        {
            _customerRepository = customerRepository;
            _daprClient = daprClient;
        }

        public async Task<Response> Handle(UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id);

            customer.UpdateName(request.Name);

            await _customerRepository.UpdateAsync(customer, cancellationToken);

            var customerUpdated = new CustomerUpdated(customer.Identification.Id, customer.Name, customer.Email.Value);

            _ = _daprClient.PublishEventAsync("play-customer-pub-sub", Topics.CustomerUpdated,
                customerUpdated, cancellationToken);

            return Response.Ok(StatusCodes.Status204NoContent);
        }
    }
}