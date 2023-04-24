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

    internal sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Response>
    {
        private readonly DaprClient _daprClient;
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerCommandHandler(ILogger<UpdateCustomerCommandHandler> logger,
            ICustomerRepository customerRepository, DaprClient daprClient)
        {
            _customerRepository = customerRepository;
            _daprClient = daprClient;
        }

        public async Task<Response> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(command.Id);

            customer.UpdateName(command.Name);

            await _customerRepository.UpdateAsync(customer, cancellationToken);

            var customerUpdated = new CustomerUpdated(customer.Identification.Id, customer.Name, customer.Email.Value);

            _ = _daprClient.PublishEventAsync("play-customer-service-pubsub", Topics.CustomerUpdated,
                customerUpdated, cancellationToken);

            return Response.Ok(StatusCodes.Status204NoContent);
        }
    }
}