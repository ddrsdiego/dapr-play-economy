namespace Play.Customer.Core.Application.IoC
{
    using System.Text.Json;
    using Common.Application.UseCase;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UseCases.GetCustomerById;
    using UseCases.RegisterNewCustomer;

    public static class PlayCustomerServiceContainer
    {
        public static IServiceCollection AddPlayCustomerServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddSwagger();
            services.AddDaprClient(configure => configure.UseJsonSerializationOptions(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            }));

            services.AddRepositories(configuration);
            services.AddMediatR(typeof(GetCustomerByIdRequest));
            services.AddMediatR(typeof(IdentifiedCommand<RegisterNewCustomerRequest>));
            return services;
        }
    }
}