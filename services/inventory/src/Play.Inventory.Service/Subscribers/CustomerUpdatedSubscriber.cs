namespace Play.Inventory.Service.Subscribers
{
    using System.Text.Json.Serialization;
    using Common.Application;
    using Common.Application.UseCase;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Core.Application.Helpers.Constants;
    using Core.Application.UseCases.CustomerUpdated;

    public readonly struct CustomerUpdated
    {
        [JsonConstructor]
        public CustomerUpdated(string customerId, string name, string email)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
        }

        public string CustomerId { get; }
        public string Name { get; }
        public string Email { get; }
    }

    internal static class CustomerUpdatedSubscriber
    {
        public static void HandleCustomerUpdated(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost($"/{Topics.CustomerUpdated}", async context =>
            {
                var cancellationToken = context.RequestAborted;

                var customerUpdated = await context.ReadFromBodyAsync<CustomerUpdated>();
                if (customerUpdated.IsFailure)
                    await context.Response.WriteAsync("", cancellationToken: cancellationToken);

                var customerUpdatedUseCase =
                    context.RequestServices.GetRequiredService<IUseCaseExecutor<CustomerUpdatedReq>>();

                var response = await customerUpdatedUseCase.SendAsync(new CustomerUpdatedReq(
                        customerUpdated.Value.CustomerId, customerUpdated.Value.Name, customerUpdated.Value.Email),
                    cancellationToken);

                if (response.IsFailure)
                    await context.Response.WriteAsync("", cancellationToken: cancellationToken);

                await response.WriteToPipeAsync(context.Response, cancellationToken: cancellationToken);
            }).WithTopic(DaprSettings.PubSub.Name, Topics.CustomerUpdated);
        }
    }
}