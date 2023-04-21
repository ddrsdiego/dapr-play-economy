namespace Play.Catalog.Service.Controllers.v1
{
    using System.Net;
    using System.Threading.Tasks;
    using Common.Api;
    using Common.Application;
    using Core.Application.UseCases.CreateNewCatalogItem;
    using Core.Application.UseCases.GetCatalogItemById;
    using Core.Application.UseCases.UpdateUnitPriceCatalogItem;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/items")]
    public class ItemsController : ControllerBase
    {
        private readonly ISender _sender;

        public ItemsController(ISender sender) => _sender = sender;

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetCatalogItemByIdResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        [MetricsApiType("items-catalog-get-by-id", MetricsApiTypeLevel.Public, "products")]
        public ValueTask GetById(string id)
        {
            var response = _sender.Send(new GetCatalogItemByIdRequest(id));
            return response.WriteToPipeAsync(Response);
        }

        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CreateNewCatalogItemResponse), (int) HttpStatusCode.Created)]
        public async Task<IActionResult> PostAsync([FromBody] CreateNewCatalogItemRequest request)
        {
            var response = await _sender.Send(request);
            if (response.IsFailure)
                return BadRequest(response.ErrorResponse);

            var createNewCatalogItemResponse = response.Content.GetRaw<CreateNewCatalogItemResponse>();
            
            return CreatedAtAction(nameof(GetById), new {id = createNewCatalogItemResponse.Id},
                createNewCatalogItemResponse);
        }

        [HttpPut("{id}/unit-price")]
        public ValueTask PutAsync(string id, decimal unitPrice)
        {
            var response = _sender.Send(new UpdateUnitPriceCatalogItemRequest(id, unitPrice));
            return response.WriteToPipeAsync(Response);
        }
    }
}