namespace Play.Inventory.Core.Application.UseCases.GetCustomerById
{
    using Common.Application;
    using MediatR;

    public readonly struct GetCustomerByIdQuery : IRequest<Response>
    {
        public GetCustomerByIdQuery(string userId) => UserId = userId;

        public string UserId { get; }
    }
}