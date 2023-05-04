namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate;

using Common.Domain.SeedWorks;

public record CustomerNameUpdated(string CustomerId, string Name) : INotification;