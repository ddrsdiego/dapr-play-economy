namespace Play.Customer.Core.Application.IoC
{
    using System.Text.Json;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterNewCustomerCommandHandler>());
            return services;
        }
    }
}