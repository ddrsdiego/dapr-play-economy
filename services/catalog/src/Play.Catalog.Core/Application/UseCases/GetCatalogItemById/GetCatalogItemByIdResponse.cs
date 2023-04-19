namespace Play.Catalog.Core.Application.UseCases.GetCatalogItemById
{
    using System.Text.Json.Serialization;

    public readonly struct GetCatalogItemByIdResponse
    {
        public string Id { get; }
        public string? Name { get; }
        public string? Description { get; }
        public decimal Price { get; }
        public DateTimeOffset CreatedAt { get; }

        [JsonConstructor]
        public GetCatalogItemByIdResponse(string id, string? name, string? description, decimal price,
            DateTimeOffset createdAt)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            CreatedAt = createdAt;
        }
    }
}