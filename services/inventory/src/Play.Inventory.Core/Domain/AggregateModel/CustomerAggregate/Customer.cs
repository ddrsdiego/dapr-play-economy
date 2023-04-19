namespace Play.Inventory.Core.Domain.AggregateModel.CustomerAggregate
{
    public sealed class Customer
    {
        private Customer()
            : this(string.Empty, string.Empty, string.Empty)
        {
        }

        public Customer(string customerId, string name, string email)
            : this(customerId, name, email, DateTimeOffset.UtcNow)
        {
        }

        internal Customer(string customerId, string name, string email, DateTimeOffset createdAt)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
            CreatedAt = createdAt;
        }

        public static Customer Default => new();

        public string CustomerId { get; }
        public string Name { get; }
        public string Email { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}