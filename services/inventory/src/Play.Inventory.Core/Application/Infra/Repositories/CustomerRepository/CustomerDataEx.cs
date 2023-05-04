namespace Play.Inventory.Core.Application.Infra.Repositories.CustomerRepository;

using Domain.AggregateModel.CustomerAggregate;

public static class CustomerDataEx
{
    public static CustomerData ToStateEntry(this Customer customer)
    {
        return new CustomerData(customer.CustomerId)
        {
            Id = customer.CustomerId,
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt
        };
    }

    public static Customer ToCustomer(this CustomerData data)
    {
        return new Customer(data.CustomerId, data.Name, data.Email, data.CreatedAt);
    }
}