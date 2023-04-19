namespace Play.Inventory.Core.Domain.AggregateModel.CatalogItemAggregate
{
    public class CatalogItem
    {
        private CatalogItem()
            : this(string.Empty, string.Empty, string.Empty)
        {
        }

        public CatalogItem(string catalogItemId, string name, string description)
            : this(catalogItemId, name, description, DateTimeOffset.UtcNow)
        {
        }

        internal CatalogItem(string catalogItemId, string name, string description, DateTimeOffset createdAt)
        {
            CatalogItemId = catalogItemId;
            Name = name;
            Description = description;
            CreatedAt = createdAt;
        }

        public static CatalogItem Default => new();

        public string CatalogItemId { get; }
        public string Name { get; }
        public string Description { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}