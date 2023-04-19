namespace Play.Inventory.Core.Application.Infra.Clients
{
    using System.Text.Json.Serialization;

    public record CatalogClientItemResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description
    );
}