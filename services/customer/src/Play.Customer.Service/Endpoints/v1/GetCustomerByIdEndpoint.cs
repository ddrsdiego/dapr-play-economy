namespace Play.Customer.Service.Endpoints.v1
{
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.ApiEndpoints;
    using Common.Application;
    using Core.Application.UseCases.GetCustomerById;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public sealed class GetCustomerByIdEndpoint : EndpointBaseAsync
        .WithRequest<string>
        .WithoutResult
    {
        private readonly ISender _sender;

        public GetCustomerByIdEndpoint(ISender sender) => _sender = sender;

        [HttpGet("api/v1/customers/{id}")]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(GetCustomerByIdResponse), (int) HttpStatusCode.OK,
            MediaTypeNames.Application.Json)]
        public override Task HandleAsync(string id, CancellationToken cancellationToken = new())
        {
            var rsp = _sender.Send(new GetCustomerByIdRequest(id), cancellationToken);
            return rsp.WriteToPipeAsync(Response, cancellationToken: cancellationToken).AsTask();
        }
    }
}