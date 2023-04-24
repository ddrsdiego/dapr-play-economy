namespace Play.Customer.Service.Controllers.v1
{
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Application;
    using Core.Application.UseCases.GetCustomerById;
    using Core.Application.UseCases.UpdateCustomer;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ISender _sender;

        public CustomersController(ISender sender) => _sender = sender;
        
        [HttpGet("{id}")]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetCustomerByIdResponse), (int) HttpStatusCode.OK,
            MediaTypeNames.Application.Json)]
        public ValueTask GetCustomerById(string id, CancellationToken cancellationToken = new())
        {
            var response = _sender.Send(new GetCustomerByIdRequest(id), cancellationToken);
            return response.WriteToPipeAsync(Response, cancellationToken: cancellationToken);
        }

        [HttpPut("{id}")]
        public ValueTask UpdateCustomerRequest(string id, [FromBody] UpdateCustomer request)
        {
            var response = _sender.Send(new UpdateCustomerRequest(id, request.Name));
            return response.WriteToPipeAsync(Response);
        }

        public sealed class UpdateCustomer
        {
            public string Name { get; set; }
        }
    }
}