namespace Play.Inventory.Core.Application.UseCases.GetCustomerById
{
    using Common.Application.UseCase;

    public class GetCustomerByIdRequest : UseCaseRequest
    {
        public GetCustomerByIdRequest(string userId) => UserId = userId;

        public string UserId { get; }
    }
}