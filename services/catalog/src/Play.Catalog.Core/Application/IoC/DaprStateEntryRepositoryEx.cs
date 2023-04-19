namespace Play.Catalog.Core.Application.IoC
{
    using Common.Application.Infra.Repositories.Dapr;
    using Dapr.Client;
    using Infra.Repositories;
    using Microsoft.Extensions.DependencyInjection;

    public static class DaprStateEntryRepositoryEx
    {
        public static IServiceCollection AddDaprStateEntryRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IDaprStateEntryRepository<CatalogItemData>>(sp =>
            {
                var daprClient = sp.GetRequiredService<DaprClient>();
                return new CatalogItemDaprRepository(daprClient);
            });
            return services;
        }
    }
}