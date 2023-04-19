namespace Play.Customer.Service.Controllers.v1
{
    using System.Threading.Tasks;
    using Common.Application;
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

        [HttpPut("{id}")]
        public ValueTask UpdateCustomerAsync(string id, [FromBody] UpdateCustomer request)
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