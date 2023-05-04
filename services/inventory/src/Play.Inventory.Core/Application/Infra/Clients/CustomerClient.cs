namespace Play.Inventory.Core.Application.Infra.Clients;

using System.Net;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

public record GetCustomerByIdResponse(string CustomerId, string Name, string Email);

public interface ICustomerClient
{
    Task<Result<GetCustomerByIdResponse>> GetCustomerByIdAsync(string userId);
}

internal sealed class CustomerClient : ICustomerClient
{
    public const string PlayCustomerServiceName = "play-customer-service";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CustomerClient> _logger;

    public CustomerClient(ILogger<CustomerClient> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<GetCustomerByIdResponse>> GetCustomerByIdAsync(string userId)
    {
        var client = _httpClientFactory.CreateClient(PlayCustomerServiceName);

        try
        {
            var response = await client.GetAsync(CustomerClientEndpoints.GetCustomerById(userId));
            if (response.StatusCode != HttpStatusCode.OK)
                return Result.Failure<GetCustomerByIdResponse>("");

            var content = await response.Content.ReadAsStringAsync();
            var customerResponse = JsonSerializer.Deserialize<GetCustomerByIdResponse>(content,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

            return Result.Success(customerResponse);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "");
            return Result.Failure<GetCustomerByIdResponse>(e.Message);
        }
    }
}

internal static class CustomerClientEndpoints
{
    public static string GetCustomerById(string userId) =>
        $"v1.0/invoke/{CustomerClient.PlayCustomerServiceName}/method/api/v1/customers/{userId}";
}