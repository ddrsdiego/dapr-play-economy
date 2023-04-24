﻿namespace Play.Customer.Core.Application.UseCases.RegisterNewCustomer
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Application;
    using Dapr.Client;
    using Domain.AggregateModel.CustomerAggregate;
    using Helpers.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    internal sealed class RegisterNewCustomerCommandHandler : IRequestHandler<RegisterNewCustomerRequest, Response>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<RegisterNewCustomerCommandHandler> _logger;
        private readonly ICustomerRepository _customerRepository;

        public RegisterNewCustomerCommandHandler(ILogger<RegisterNewCustomerCommandHandler> logger,
            DaprClient daprClient, ICustomerRepository customerRepository)
        {
            _logger = logger;
            _daprClient = daprClient;
            _customerRepository = customerRepository;
        }

        public async Task<Response> Handle(RegisterNewCustomerRequest request, CancellationToken cancellationToken)
        {
            if (await TryFindAlreadyRegisteredCustomers(request))
            {
                _logger.LogWarning("");
                return Response.Fail("ERROR_USER_ALREADY_REGISTERED", "User already registered.");
            }

            var newCustomer = new Customer(request.Document, request.Name, request.Email);
            await _customerRepository.SaveAsync(newCustomer, cancellationToken);

            foreach (var notification in newCustomer.DomainEvents)
            {
                var @event = (NewCustomerCreated) notification;
                await _daprClient.PublishEventAsync(DaprSettings.PubSub.Name, Topics.CustomerRegistered,
                    @event, cancellationToken);
            }

            var responseContent = new RegisterNewCustomerResponse(newCustomer.Identification.Id, newCustomer.Name,
                newCustomer.Email.Value,
                newCustomer.CreatedAt);

            return Response.Ok(ResponseContent.Create(responseContent), StatusCodes.Status201Created,
                request.RequestId);
        }

        private async Task<bool> TryFindAlreadyRegisteredCustomers(RegisterNewCustomerRequest request)
        {
            var getByEmailTask = _customerRepository.GetByEmailAsync(request.Email);
            var getByDocumentTask = _customerRepository.GetByEmailAsync(request.Document);

            var tasks = new List<Task<Customer>>(2) {getByEmailTask, getByDocumentTask};

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                var customer = await task;

                if (customer.IsValidCustomer)
                    return customer.IsValidCustomer;
            }

            return false;
        }
    }
}