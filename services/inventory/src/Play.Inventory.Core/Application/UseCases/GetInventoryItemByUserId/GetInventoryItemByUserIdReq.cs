namespace Play.Inventory.Core.Application.UseCases.GetInventoryItemByUserId
{
    using Common.Application.UseCase;

    public class GetInventoryItemByUserIdReq : UseCaseRequest
    {
        public GetInventoryItemByUserIdReq(string userId) => UserId = userId;

        public string UserId { get; }
    }
}