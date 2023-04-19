namespace Play.Customer.Core.Application.UseCases.GetCustomerById
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Application;
    using Domain.AggregateModel.CustomerAggregate;
    using MediatR;
    using Microsoft.Extensions.Logging;

    internal sealed class GetCustomerByIdQuery : IRequestHandler<GetCustomerByIdRequest, Response>
    {
        private readonly ILogger<GetCustomerByIdQuery> _logger;
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdQuery(ILogger<GetCustomerByIdQuery> logger, ICustomerRepository customerRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
        }

        public async Task<Response> Handle(GetCustomerByIdRequest request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id);
            if (customer.Identification.Id == Customer.Default.Identification.Id)
            {
                var error = new Error("CUSTOMER_NOT_FOUND", $"Client not found for id {request.Id}");
                return Response.Fail(error);
            }

            var response =
                new GetCustomerByIdResponse(customer.Identification.Id, customer.Name, customer.Email.Value,
                    customer.CreatedAt);

            return Response.Ok(ResponseContent.Create(response));
        }
    }
}