namespace Play.Inventory.Core.Application.UseCases.GetCustomerById
{
    using Common.Application;
    using MediatR;

    public readonly struct GetCustomerByIdRequest : IRequest<Response>
    {
        public GetCustomerByIdRequest(string userId) => UserId = userId;

        public string UserId { get; }
    }
}