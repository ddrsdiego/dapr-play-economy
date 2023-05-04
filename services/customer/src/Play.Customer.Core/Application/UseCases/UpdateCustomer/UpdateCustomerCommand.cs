namespace Play.Customer.Core.Application.UseCases.UpdateCustomer;

using Common.Application;
using MediatR;

public sealed record UpdateCustomerCommand(string UserId, string Name) : IRequest<Response>
{
    public readonly string UserId = UserId;
    public readonly string Name = Name;
}