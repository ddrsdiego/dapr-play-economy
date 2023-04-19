namespace Play.Inventory.Core.Application.Infra.Repositories.CatalogItemRepository
{
    using Dapr.Client;
    using Microsoft.Extensions.Options;
    using Play.Common.Application.Infra.Repositories.Dapr;

    public sealed class CatalogItemDaprStateRepository : DaprStateEntryRepository<CatalogItemData>
    {
        public CatalogItemDaprStateRepository(DaprClient daprClient, IOptions<AppSettings> options)
            : base(options.Value.DaprSettings.StateStoreName, daprClient)
        {
        }
    }
}