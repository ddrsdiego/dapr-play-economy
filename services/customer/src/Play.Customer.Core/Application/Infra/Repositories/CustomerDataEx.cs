namespace Play.Customer.Core.Application.Infra.Repositories;

using Domain.AggregateModel.CustomerAggregate;

internal static class CustomerDataEx
{
    public static Customer ToCustomer(this CustomerData data) =>
        new(data.CustomerId, data.Document, data.Name, data.Email, data.CreatedAt);

    public static CustomerData ToCustomerData(this Customer customer)
    {
        return new CustomerData
        {
            Id = customer.Identification.Id,
            CustomerId = customer.Identification.Id,
            Document = customer.Document.Value,
            Name = customer.Name,
            Email = customer.Email.Value,
            CreatedAt = customer.CreatedAt
        };
    }
}