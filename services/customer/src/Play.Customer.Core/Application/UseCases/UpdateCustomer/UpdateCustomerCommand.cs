namespace Play.Customer.Core.Application.UseCases.UpdateCustomer
{
    using Common.Application;
    using MediatR;

    public sealed record UpdateCustomerCommand(string Id, string Name) : IRequest<Response>
    {
        public readonly string Id = Id;
        public readonly string Name = Name;
    }
}