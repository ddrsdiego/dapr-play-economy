namespace Play.Inventory.Core.Application.IoC
{
    using Common.Application.Infra.Repositories.Dapr;
    using Infra.Repositories.CatalogItemRepository;
    using Infra.Repositories.CustomerRepository;
    using Infra.Repositories.InventoryItemRepository;
    using Microsoft.Extensions.DependencyInjection;

    internal static class RepositoriesContainer
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IDaprStateEntryRepository<CustomerData>, CustomerDaprStateRepository>();
            services.AddSingleton<IDaprStateEntryRepository<CatalogItemData>, CatalogItemDaprStateRepository>();
            services.AddSingleton<IDaprStateEntryRepository<InventoryItemData>, InventoryItemDaprStateRepository>();
            
            return services;
        }
    }
}