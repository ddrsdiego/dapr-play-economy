namespace Play.Customer.Core.Application.UseCases.UpdateCustomer;

using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Application;
using Common.Application.Infra;
using Common.Application.Infra.Outbox;
using Domain.AggregateModel.CustomerAggregate;
using Helpers.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public static class Errors
{
    public static class Customer
    {
        public static Error UserNotFound(string id) => new("USER_NOT_FOUND", $"Client not found for id {id}");

        public static Error UserAlreadyExists(string email) =>
            new("USER_ALREADY_EXISTS", $"User already exists for email {email}");
    }
}

internal sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Response>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOutboxMessagesRepository _outboxMessagesRepository;

    public UpdateCustomerCommandHandler(ILogger<UpdateCustomerCommandHandler> logger,
        ICustomerRepository customerRepository, IOutboxMessagesRepository outboxMessagesRepository,
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _customerRepository = customerRepository;
        _outboxMessagesRepository = outboxMessagesRepository;
    }

    public async Task<Response> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(command.UserId);
            if (customer.HasNoValue)
                return Response.Fail(Errors.Customer.UserNotFound(command.UserId));


            customer.Value.UpdateName(command.Name);

            await using var uow = _unitOfWorkFactory.Create(cancellationToken);

            var customerNameUpdated = new CustomerNameUpdated(customer.Value.Identification.Id, customer.Value.Name);

            await _customerRepository.UpdateAsync(customer.Value, cancellationToken);
            await _outboxMessagesRepository.SaveAsync(nameof(CustomerNameUpdated), Topics.CustomerUpdated,
                customerNameUpdated, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Response.Ok(StatusCodes.Status500InternalServerError);
        }

        return Response.Ok(StatusCodes.Status204NoContent);
    }
}