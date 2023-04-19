namespace Play.Catalog.Core.Domain.AggregatesModel.CatalogItemAggregate
{
    public readonly struct CatalogItemDescription
    {
        public CatalogItemDescription(string? name, string? value)
        {
            Name = name;
            Value = value;
        }

        public string? Name { get; }
        public string? Value { get; }
    }
}