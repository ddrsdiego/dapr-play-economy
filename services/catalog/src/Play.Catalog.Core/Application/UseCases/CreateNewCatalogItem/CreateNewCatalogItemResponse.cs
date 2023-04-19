namespace Play.Catalog.Core.Application.UseCases.CreateNewCatalogItem
{
    using System.Text.Json.Serialization;

    public readonly struct CreateNewCatalogItemResponse
    {
        [JsonConstructor]
        public CreateNewCatalogItemResponse(string id, string? name, string? description, decimal unitPrice,
            DateTimeOffset createdAt)
        {
            Id = id;
            Name = name;
            Description = description;
            UnitPrice = unitPrice;
            CreatedAt = createdAt;
        }

        public string Id { get; }
        public string? Name { get; }
        public string? Description { get; }
        public decimal UnitPrice { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}