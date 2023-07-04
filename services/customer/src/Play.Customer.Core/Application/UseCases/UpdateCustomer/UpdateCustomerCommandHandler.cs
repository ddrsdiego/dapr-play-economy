namespace Play.Customer.Core.Application.UseCases.UpdateCustomer;

using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Application;
using Common.Application.Infra.UoW;
using Common.Application.Messaging.OutBox;
using Domain.AggregateModel.CustomerAggregate;
using Helpers.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;

public static class Errors
{
    public static class Customer
    {
        public static Error UserNotFound(string id) => new("USER_NOT_FOUND", $"User not found for id {id}");

        public static Error UserAlreadyExists(string email) => new("USER_ALREADY_EXISTS", $"User already exists for email {email}");
    }
}

internal sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Response>
{
    private const string PubSubName = "play-customer-service-pubsub";

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOutBoxMessagesRepository _outBoxMessagesRepository;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository, IOutBoxMessagesRepository outBoxMessagesRepository, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _customerRepository = customerRepository;
        _outBoxMessagesRepository = outBoxMessagesRepository;
    }

    public async Task<Response> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(command.UserId);
            if (customer.HasNoValue)
                return Response.Fail(Errors.Customer.UserNotFound(command.UserId));

            customer.Value.UpdateName(command.Name);
            var customerNameUpdated = new CustomerNameUpdated(customer.Value.Identification.Id, customer.Value.Name);

            await using var uow = await _unitOfWorkFactory.CreateAsync();

            await uow.AddToContextAsync(async () => await _customerRepository.UpdateAsync(customer.Value, cancellationToken));
            await uow.AddToContextAsync(async () => await _outBoxMessagesRepository.SaveAsync(PubSubName, nameof(CustomerNameUpdated), Topics.CustomerUpdated, customerNameUpdated, cancellationToken));

            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Response.Ok(StatusCodes.Status500InternalServerError);
        }

        return Response.Ok(StatusCodes.Status204NoContent);
    }
}