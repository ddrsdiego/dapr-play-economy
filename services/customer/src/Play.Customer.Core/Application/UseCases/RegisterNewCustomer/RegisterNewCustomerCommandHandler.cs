namespace Play.Customer.Core.Application.UseCases.RegisterNewCustomer;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Application;
using Common.Application.Infra;
using Common.Application.Infra.Outbox;
using CSharpFunctionalExtensions;
using Dapr.Client;
using Domain.AggregateModel.CustomerAggregate;
using Helpers.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UpdateCustomer;

internal sealed class RegisterNewCustomerCommandHandler : IRequestHandler<RegisterNewCustomerCommand, Response>
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<RegisterNewCustomerCommandHandler> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOutboxMessagesRepository _outboxMessagesRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public RegisterNewCustomerCommandHandler(ILogger<RegisterNewCustomerCommandHandler> logger,
        DaprClient daprClient, ICustomerRepository customerRepository,
        IOutboxMessagesRepository outboxMessagesRepository, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _daprClient = daprClient;
        _customerRepository = customerRepository;
        _outboxMessagesRepository = outboxMessagesRepository;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<Response> Handle(RegisterNewCustomerCommand command, CancellationToken cancellationToken)
    {
        if (await TryFindAlreadyRegisteredCustomers(command))
            return Response.Fail(Errors.Customer.UserAlreadyExists(command.Email));

        var newCustomer = new Customer(command.Document, command.Name, command.Email);
        await using var uow = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        await _customerRepository.SaveAsync(newCustomer, cancellationToken);
        foreach (var notification in newCustomer.DomainEvents)
        {
            var @event = (NewCustomerCreated) notification;
            await _outboxMessagesRepository.SaveAsync(nameof(NewCustomerCreated), Topics.CustomerRegistered, @event,
                cancellationToken);
        }

        var responseContent = new RegisterNewCustomerResponse(newCustomer.Identification.Id, newCustomer.Name,
            newCustomer.Email.Value,
            newCustomer.CreatedAt);

        return Response.Ok(ResponseContent.Create(responseContent), StatusCodes.Status201Created,
            command.RequestId);
    }

    private async Task<bool> TryFindAlreadyRegisteredCustomers(RegisterNewCustomerCommand command)
    {
        var getByEmailTask = _customerRepository.GetByEmailAsync(command.Email);
        var getByDocumentTask = _customerRepository.GetByDocumentAsync(command.Document);

        var tasks = new List<Task<Maybe<Customer>>>(2) {getByEmailTask, getByDocumentTask};

        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            var customer = await task;

            if (customer.HasValue) return customer.HasValue;
        }

        return false;
    }
}