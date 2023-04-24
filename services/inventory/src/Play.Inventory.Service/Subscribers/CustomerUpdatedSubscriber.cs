namespace Play.Inventory.Service.Subscribers
{
    using System.Text.Json.Serialization;
    using Common.Application;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Core.Application.Helpers.Constants;
    using Core.Application.UseCases.CustomerUpdated;
    using MediatR;

    public readonly struct CustomerUpdatedRequest
    {
        [JsonConstructor]
        public CustomerUpdatedRequest(string customerId, string name, string email)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
        }

        public string CustomerId { get; }
        public string Name { get; }
        public string Email { get; }
        
        public CustomerUpdatedCommand ToCommand() => new(CustomerId, Name, Email);
    }

    internal static class CustomerUpdatedSubscriber
    {
        public static void HandleCustomerUpdated(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost($"/{Topics.CustomerUpdated}", async context =>
            {
                var cancellationToken = context.RequestAborted;

                var customerUpdatedRequest = await context.ReadFromBodyAsync<CustomerUpdatedRequest>();
                if (customerUpdatedRequest.IsFailure)
                    await context.Response.WriteAsync("", cancellationToken: cancellationToken);
                
                var sender = context.RequestServices.GetRequiredService<ISender>();

                var response = await sender.Send(customerUpdatedRequest.Value.ToCommand(), cancellationToken);
                if (response.IsFailure)
                    await context.Response.WriteAsync("", cancellationToken: cancellationToken);

                await response.WriteToPipeAsync(context.Response, cancellationToken: cancellationToken);
            }).WithTopic(DaprSettings.PubSub.Name, Topics.CustomerUpdated);
        }
    }
}