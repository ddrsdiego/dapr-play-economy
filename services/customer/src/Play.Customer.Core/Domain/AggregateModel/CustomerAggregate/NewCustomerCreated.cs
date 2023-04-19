namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate
{
    using System;
    using Common.Domain.SeedWorks;

    public record NewCustomerCreated(string CustomerId, string Name, string Email, DateTimeOffset CreatedAt) : INotification;
}