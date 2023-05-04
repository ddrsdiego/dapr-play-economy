namespace Play.Inventory.Service.Controllers.v1;

using System.Threading.Tasks;
using Common.Application;
using Microsoft.AspNetCore.Mvc;
using Common.Application.UseCase;
using Core.Application.UseCases.GetInventoryItemByUserId;
using MediatR;
using Requests;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/items")]
public class ItemController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUseCaseExecutor<GetInventoryItemByUserIdReq> _getInventoryItemByUserIdUseCase;

    public ItemController(ISender sender,
        IUseCaseExecutor<GetInventoryItemByUserIdReq> getInventoryItemByUserIdUseCase)
    {
        _sender = sender;
        _getInventoryItemByUserIdUseCase = getInventoryItemByUserIdUseCase;
    }

    [HttpGet("{userId}")]
    public ValueTask GetByUserId(string userId)
    {
        var response = _getInventoryItemByUserIdUseCase.SendAsync(new GetInventoryItemByUserIdReq(userId));
        return response.WriteToPipeAsync(Response);
    }

    [HttpPost]
    public ValueTask GrantItemAsync([FromBody] GrantItemRequest request)
    {
        var response = _sender.Send(request.ToGrantItemCommand());
        return response.WriteToPipeAsync(Response);
    }
}