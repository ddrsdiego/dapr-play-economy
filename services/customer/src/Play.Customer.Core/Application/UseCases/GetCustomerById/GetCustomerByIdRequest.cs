namespace Play.Customer.Core.Application.UseCases.GetCustomerById;

using Common.Application;
using MediatR;

public readonly struct GetCustomerByIdRequest : IRequest<Response>
{
    public GetCustomerByIdRequest(string id) => Id = id;

    public string Id { get; }
}