namespace Play.Customer.Core.Application.IoC;

using System;
using System.Data.Common;
using Common.Application.Infra.Repositories;
using Common.Application.Infra.UoW;
using Common.Application.Messaging;
using Common.Application.Messaging.InBox;
using Common.Application.Messaging.OutBox;
using Common.Application.UseCase;
using Dapr.Client;
using Domain.AggregateModel.CustomerAggregate;
using Infra.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Polly;

internal static class RepositoriesContainer
{
    private const string PostgresConnectionStringSection = "ConnectionStringOptions:PostgresConnection";

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICustomerRepository, CustomerRepository>();
        services.AddSingleton<IRequestManager, RequestManagerMemoryCache>();

        services.TryAddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
        services.TryAddSingleton<IConnectionManager>(sp =>
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
            var connectionString = sp.GetRequiredService<IConfiguration>().GetSection(PostgresConnectionStringSection).Value;
            return new ConnectionManager(npgsqlFactory, connectionString, resiliencePolicy);
        });

        services.TryAddSingleton<IUnitOfWorkFactory>(sp =>
        {
            var connectionManager = sp.GetRequiredService<IConnectionManager>();
            return new UnitOfWorkFactory(connectionManager);
        });

        services.TryAddSingleton<IInBoxMessagesProcessor>(sp =>
            new InBoxMessagesProcessor(new BoxMessagesProcessorConfig
                {
                    PubSubName = "play-customer-service-pubsub",
                    LockStoreName = "play-customer-service-lock-store",
                    ProcessingIntervalInSeconds = 5
                },
                sp.GetRequiredService<IInBoxMessagesRepository>(),
                sp.GetRequiredService<IUnitOfWorkFactory>(),
                sp.GetRequiredService<DaprClient>()));
        
        services.TryAddSingleton<IOutBoxMessagesProcessor>(sp =>
            new OutBoxMessagesProcessor(new BoxMessagesProcessorConfig
                {
                    PubSubName = "play-customer-service-pubsub",
                    LockStoreName = "play-customer-service-lock-store",
                    ProcessingIntervalInSeconds = 5
                },
                sp.GetRequiredService<IOutBoxMessagesRepository>(),
                sp.GetRequiredService<IUnitOfWorkFactory>(),
                sp.GetRequiredService<DaprClient>()));

        services.TryAddSingleton<IOutBoxMessagesRepository>(sp =>
            new OutBoxMessagesRepository(sp.GetRequiredService<IConnectionManager>()));

        return services;
    }
}