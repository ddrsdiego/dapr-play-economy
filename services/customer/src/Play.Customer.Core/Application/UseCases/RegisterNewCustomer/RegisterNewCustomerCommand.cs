namespace Play.Customer.Core.Application.UseCases.RegisterNewCustomer
{
    using Common.Application;
    using MediatR;

    public record struct RegisterNewCustomerCommand
        (string RequestId, string Document, string Name, string Email) : IRequest<Response>;
}