namespace Play.Catalog.Core.Application.Infra.Repositories
{
    using System.Text.Json.Serialization;
    using Common.Application.Infra.Repositories.Dapr;
    using Dapr.Client;

    [StateEntryName("catalog-item")]
    public class CatalogItemData : DaprStateEntry
    {
        public const string StateStoreName = "play-catalog-state-store";

        [JsonConstructor]
        public CatalogItemData(string? stateEntryKey)
            : base(stateEntryKey)
        {
        }
        
        public string? CatalogItemId { get; set; }
        public string? CatalogItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreateAt { get; set; }
    }

    public sealed class CatalogItemDaprRepository : DaprStateEntryRepository<CatalogItemData>
    {
        public CatalogItemDaprRepository(DaprClient daprClient)
            : base(CatalogItemData.StateStoreName, daprClient)
        {
        }
    }
}