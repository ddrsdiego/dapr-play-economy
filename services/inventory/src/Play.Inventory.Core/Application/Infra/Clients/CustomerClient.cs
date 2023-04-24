namespace Play.Inventory.Core.Application.Infra.Clients
{
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using CSharpFunctionalExtensions;
    using Dapr.Client;
    using Microsoft.Extensions.Logging;

    public record GetCustomerByEmailResponse(string CustomerId, string Name, string Email);

    public interface ICustomerClient
    {
        Task<Result<GetCustomerByEmailResponse>> GetCustomerById(string userId);
    }

    public class CustomerClient : ICustomerClient
    {
        public const string PlayCustomerServiceName = "play-customer-service";
        private readonly ILogger<CustomerClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DaprClient _daprClient;

        public CustomerClient(ILogger<CustomerClient> logger, IHttpClientFactory httpClientFactory,
            DaprClient daprClient)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _daprClient = daprClient;
        }

        public async Task<Result<GetCustomerByEmailResponse>> GetCustomerById(string userId)
        {
            // var client = _httpClientFactory.CreateClient(PlayCustomerServiceName);

            var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"http://localhost:{daprHttpPort}");
            client.DefaultRequestHeaders.Add("dapr-app-id", PlayCustomerServiceName);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            try
            {
                var rsp = await _daprClient.InvokeMethodAsync<GetCustomerByEmailResponse>(HttpMethod.Get, PlayCustomerServiceName, $"/{userId}");
                var response =
                    await client.GetAsync($"v1.0/invoke/{PlayCustomerServiceName}/method/api/v1/customers/{userId}");
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return Result.Failure<GetCustomerByEmailResponse>("");
                }

                var content = await response.Content.ReadAsStringAsync();
                var customerResponse = JsonSerializer.Deserialize<GetCustomerByEmailResponse>(content,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

                return Result.Success(customerResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                return Result.Failure<GetCustomerByEmailResponse>(e.Message);
            }
        }
    }
}