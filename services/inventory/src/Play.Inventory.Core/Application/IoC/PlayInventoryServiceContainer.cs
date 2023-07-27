namespace Play.Inventory.Core.Application.IoC;

using Common.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UseCases.CreateCatalogItem;

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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateCatalogItemCommandHandler>());
        services.AddMessaging(configuration);
        return services;
    }
}