namespace Play.Inventory.Core.Application.Infra.Repositories.CustomerRepository
{
    using Common.Application.Infra.Repositories.Dapr;

    [StateEntryName("customer")]
    public sealed class CustomerData : IDaprStateEntry
    {
        public CustomerData(string stateEntryKey) => StateEntryKey = stateEntryKey;

        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string StateEntryKey { get; }
    }
}