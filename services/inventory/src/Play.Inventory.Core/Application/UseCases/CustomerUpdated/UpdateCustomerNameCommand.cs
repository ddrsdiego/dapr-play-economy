namespace Play.Inventory.Core.Application.UseCases.CustomerUpdated;

using System.Text.Json.Serialization;
using Common.Application;
using MediatR;

public readonly struct UpdateCustomerNameCommand : IRequest<Response>
{
    [JsonConstructor]
    public UpdateCustomerNameCommand(string customerId, string name, string email)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
    }

    public string CustomerId { get; }
    public string Name { get; }
    public string Email { get; }
}