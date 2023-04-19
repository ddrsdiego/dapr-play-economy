namespace Play.Customer.Core.Application.UseCases.RegisterNewCustomer
{
    using Common.Application;
    using Common.Application.UseCase;
    using MediatR;

    public sealed class RegisterNewCustomerRequest : UseCaseRequest, IRequest<Response>
    {
        public RegisterNewCustomerRequest(string document, string name, string email)
        {
            Document = document;
            Name = name;
            Email = email;
        }

        public string Document { get; }
        public string Name { get; }
        public string Email { get; }
    }
}