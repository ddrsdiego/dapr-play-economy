﻿namespace Play.Inventory.Core.Application.IoC;

using System.Data.Common;
using Common.Application.Infra.Repositories;
using Common.Application.Infra.Repositories.Dapr;
using Common.Application.Infra.UoW;
using Common.Application.Messaging;
using Common.Application.Messaging.InBox;
using Common.Application.Messaging.OutBox;
using Dapr.Client;
using Infra.Repositories.CatalogItemRepository;
using Infra.Repositories.CustomerRepository;
using Infra.Repositories.InventoryItemRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Polly;

internal static class RepositoriesContainer
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.TryAddSingleton<IDaprStateEntryRepository<CustomerData>, CustomerDaprStateRepository>();
        services.TryAddSingleton<IDaprStateEntryRepository<CatalogItemData>, CatalogItemDaprStateRepository>();
        services.TryAddSingleton<IDaprStateEntryRepository<InventoryItemData>, InventoryItemDaprStateRepository>();

        services.TryAddSingleton<IInBoxMessagesRepository>(sp => new InBoxMessagesRepository(sp.GetRequiredService<IConnectionManager>()));
        services.TryAddSingleton<IOutBoxMessagesRepository>(sp => new OutBoxMessagesRepository(sp.GetRequiredService<IConnectionManager>()));
        services.TryAddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
        services.TryAddSingleton(TryAddInBoxMessagesProcessor);
        services.TryAddSingleton(TryAddConnectionManager);
        services.TryAddSingleton<IUnitOfWorkFactory>(sp =>
        {
            var connectionManager = sp.GetRequiredService<IConnectionManager>();
            return new UnitOfWorkFactory(connectionManager);
        });
        
        return services;
    }

    private static IConnectionManager TryAddConnectionManager(IServiceProvider sp)
    {
        var resiliencePolicy = Policy
            .Handle<NpgsqlException>()
            .Or<DbException>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(10),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(500)
            });

        var npgsqlFactory = sp.GetRequiredService<DbProviderFactory>();
        var connectionString = sp.GetRequiredService<IConfiguration>().GetSection("ConnectionStringOptions:PostgresConnection").Value;
        return new ConnectionManager(npgsqlFactory, connectionString, resiliencePolicy);
    }

    private static IInBoxMessagesProcessor TryAddInBoxMessagesProcessor(IServiceProvider sp)
    {
        var config = new BoxMessagesProcessorConfig
        {
            PubSubName = Helpers.Constants.DaprSettings.PubSub.Name,
            LockStoreName = Helpers.Constants.DaprSettings.PubSub.LockStoreName
        };

        var daprClient = sp.GetRequiredService<DaprClient>();
        var repository = sp.GetRequiredService<IInBoxMessagesRepository>();
        var unitOfWorkFactory = sp.GetRequiredService<IUnitOfWorkFactory>();
        return new InBoxMessagesProcessor(config, repository, unitOfWorkFactory, daprClient);
    }
}