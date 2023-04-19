namespace Play.Inventory.Core.Application.Infra.Clients
{
    using System.Net;
    using System.Text.Json;
    using CSharpFunctionalExtensions;
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

        public CustomerClient(ILogger<CustomerClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<GetCustomerByEmailResponse>> GetCustomerById(string userId)
        {
            var client = _httpClientFactory.CreateClient(PlayCustomerServiceName);

            try
            {
                var response = await client.GetAsync($"/api/v1/customers/{userId}");
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.Failure<GetCustomerByEmailResponse>("");

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