namespace Play.Inventory.Core.Application.Infra.Repositories.CustomerRepository
{
    using Dapr.Client;
    using Microsoft.Extensions.Options;
    using Play.Common.Application.Infra.Repositories.Dapr;

    public sealed class CustomerDaprStateRepository : DaprStateEntryRepository<CustomerData>
    {
        public CustomerDaprStateRepository(DaprClient daprClient, IOptions<AppSettings> options)
            : base(options.Value.DaprSettings.StateStoreName, daprClient)
        {
        }
    }
}