namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate
{
    public record CustomerUpdated(string CustomerId, string Name, string Email);
}