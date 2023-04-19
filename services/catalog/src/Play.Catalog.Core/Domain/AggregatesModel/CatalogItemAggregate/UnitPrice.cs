namespace Play.Catalog.Core.Domain.AggregatesModel.CatalogItemAggregate
{
    public readonly struct UnitPrice
    {
        public UnitPrice(decimal value)
        {
            if (value <= 0)
                throw new ArgumentException();
            
            Value = value;
        }

        public decimal Value { get; }
    }
}