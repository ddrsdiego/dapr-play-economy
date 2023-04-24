namespace Play.Customer.Service.Endpoints.v1
{
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.ApiEndpoints;
    using Common.Api;
    using Core.Application.UseCases.RegisterNewCustomer;
    using Helpers.Constants;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public sealed class RegisterNewCustomerRequest
    {
        public string RequestId { get; set; }
        public string Document { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        
        public RegisterNewCustomerCommand ToCommand() => new(RequestId, Document, Name, Email);
    }

    public sealed class RegisterNewCustomerEndpoint : EndpointBaseAsync
        .WithRequest<RegisterNewCustomerRequest>
        .WithActionResult
    {
        private readonly ISender _sender;

        public RegisterNewCustomerEndpoint(ISender sender) => _sender = sender;

        [HttpPost("api/v1/customers")]
        [RequiredHeader(HeaderNames.XRequestId, HeaderDescriptions.XRequestId)]
        [ProducesResponseType(typeof(RegisterNewCustomerResponse), (int) HttpStatusCode.Created,
            MediaTypeNames.Application.Json)]
        public override async Task<ActionResult> HandleAsync(RegisterNewCustomerRequest command,
            CancellationToken cancellationToken = new())
        {
            command.RequestId = HttpContext.Request.Headers[HeaderNames.XRequestId].ToString();
            var response = await _sender.Send(command.ToCommand(), cancellationToken);
            if (response.IsFailure)
                return BadRequest(response.ErrorResponse);

            var registerResponse = response.Content.GetRaw<RegisterNewCustomerResponse>();
            return Created("", registerResponse);
        }
    }
}