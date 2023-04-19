namespace Play.Inventory.Core.Application.Infra.Repositories.InventoryItemRepository
{
    using Dapr.Client;
    using Microsoft.Extensions.Options;
    using Play.Common.Application.Infra.Repositories.Dapr;

    public sealed class InventoryItemDaprStateRepository : DaprStateEntryRepository<InventoryItemData>
    {
        public InventoryItemDaprStateRepository(DaprClient daprClient, IOptions<AppSettings> options)
            : base(options.Value.DaprSettings.StateStoreName, daprClient)
        {
        }
    }
}