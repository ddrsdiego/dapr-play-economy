namespace Play.Inventory.Core.Application.IoC
{
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using Infra.Clients;
    using Microsoft.Extensions.DependencyInjection;
    using Polly;

    public static class HttpClientContainer
    {
        public static void AddHttpClients(this IServiceCollection services)
        {
            var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");

            services.AddSingleton<ICatalogClient, CatalogClient>();
            services.AddSingleton<ICustomerClient, CustomerClient>();

            var uriSideCar = $"http://localhost:{daprHttpPort}";
            services.AddHttpClient(CatalogClient.PlayCatalogServiceName, client =>
                {
                    client.BaseAddress = new Uri(uriSideCar);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("dapr-app-id",
                        CatalogClient.PlayCatalogServiceName);
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                }).SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(5));

            services.AddHttpClient(CustomerClient.PlayCustomerServiceName, client =>
            {
                client.BaseAddress = new Uri(uriSideCar);
                client.DefaultRequestHeaders.TryAddWithoutValidation("dapr-app-id",
                    CustomerClient.PlayCustomerServiceName);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            });
        }
    }
}