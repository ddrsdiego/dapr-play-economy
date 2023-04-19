namespace Play.Inventory.Core.Application.IoC
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class PlayInventoryServiceContainer
    {
        public static IServiceCollection AddPlayInventoryServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddUseCases();
            services.AddSwagger();
            services.AddHttpClients();
            services.AddRepositories();
            services.AddOptions(configuration);

            return services;
        }
    }
}