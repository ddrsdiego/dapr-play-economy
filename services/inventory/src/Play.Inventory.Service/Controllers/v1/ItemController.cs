namespace Play.Inventory.Service.Controllers.v1
{
    using System.Threading.Tasks;
    using Common.Application;
    using Microsoft.AspNetCore.Mvc;
    using Common.Application.UseCase;
    using Core.Application.UseCases.GetInventoryItemByUserId;
    using Core.Application.UseCases.GrantItem;

    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/items")]
    public class ItemController : ControllerBase
    {
        private readonly IUseCaseExecutor<GrantItemRequest> _grantItemUseCase;
        private readonly IUseCaseExecutor<GetInventoryItemByUserIdReq> _getInventoryItemByUserIdUseCase;

        public ItemController(IUseCaseExecutor<GrantItemRequest> grantItemUseCase,
            IUseCaseExecutor<GetInventoryItemByUserIdReq> getInventoryItemByUserIdUseCase)
        {
            _grantItemUseCase = grantItemUseCase;
            _getInventoryItemByUserIdUseCase = getInventoryItemByUserIdUseCase;
        }

        [HttpGet("{userId}")]
        public ValueTask GetByUserId(string userId)
        {
            var response = _getInventoryItemByUserIdUseCase.SendAsync(new GetInventoryItemByUserIdReq(userId));
            return response.WriteToPipeAsync(Response);
        }

        [HttpPost]
        public ValueTask Post([FromBody] GrantItemRequest request)
        {
            var response = _grantItemUseCase.SendAsync(request);
            return response.WriteToPipeAsync(Response);
        }
    }
}