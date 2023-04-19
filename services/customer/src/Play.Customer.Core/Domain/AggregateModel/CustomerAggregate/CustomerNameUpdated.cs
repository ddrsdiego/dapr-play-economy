namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate
{
    using Common.Domain.SeedWorks;

    public record CustomerNameUpdated(string CustomerId, string Name) : INotification
    {
        public readonly string CustomerId = CustomerId;
        public readonly string Name = Name;
    }
}