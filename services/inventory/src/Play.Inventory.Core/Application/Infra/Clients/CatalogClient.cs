namespace Play.Inventory.Core.Application.Infra.Clients
{
    using System.Net;
    using System.Text.Json;

    public interface ICatalogClient
    {
        Task<IReadOnlyCollection<CatalogClientItemResponse>> GetCatalogItemsAsync(string[] catalogItemIds);
    }

    public sealed class CatalogClient : ICatalogClient
    {
        public const string PlayCatalogServiceName = "play-catalog-service";

        private readonly IHttpClientFactory _httpClientFactory;

        public CatalogClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyCollection<CatalogClientItemResponse>> GetCatalogItemsAsync(
            string[] catalogItemIds)
        {
            var catalogItemsResponse = new List<CatalogClientItemResponse>(catalogItemIds.Length);

            var client = _httpClientFactory.CreateClient(PlayCatalogServiceName);
            var tasks = catalogItemIds.Select(
                async catalogItemId => await client.GetAsync($"/api/v1/items/{catalogItemId}"));

            var enumerable = tasks.ToList();
            await Task.WhenAll(enumerable);

            foreach (var task in enumerable)
            {
                var response = await task;

                if (response.StatusCode != HttpStatusCode.OK)
                    continue;

                var content = await response.Content.ReadAsStringAsync();
                var catalogItem = JsonSerializer.Deserialize<CatalogClientItemResponse>(content);

                catalogItemsResponse.Add(catalogItem!);
            }

            return catalogItemsResponse.AsReadOnly();
        }
    }
}