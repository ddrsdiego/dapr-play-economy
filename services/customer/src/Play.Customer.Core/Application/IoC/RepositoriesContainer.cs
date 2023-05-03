namespace Play.Customer.Core.Application.IoC
{
    using System;
    using System.Data.Common;
    using Common.Application.Infra;
    using Common.Application.Infra.Outbox;
    using Common.Application.Infra.Repositories;
    using Common.Application.UseCase;
    using Dapr.Client;
    using Domain.AggregateModel.CustomerAggregate;
    using Infra.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
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
                var connectionString = sp.GetRequiredService<IConfiguration>()
                    .GetSection(PostgresConnectionStringSection).Value;
                return new ConnectionManager(npgsqlFactory, connectionString, resiliencePolicy);
            });

            services.TryAddSingleton<IUnitOfWorkFactory>(sp =>
            {
                var connectionManager = sp.GetRequiredService<IConnectionManager>();
                return new UnitOfWorkFactory(connectionManager);
            });

            services.TryAddSingleton<IOutboxMessagesProcessor>(sp =>
                new OutboxMessagesProcessor(new OutboxMessagesProcessorConfig
                    {
                        PubSubName = "play-customer-service-pubsub",
                        MaxProcessingMessagesCount = 100,
                        ProcessingIntervalInSeconds = TimeSpan.FromSeconds(5)
                    },
                    sp.GetRequiredService<IOutboxMessagesRepository>(),
                    sp.GetRequiredService<IUnitOfWorkFactory>(),
                    sp.GetRequiredService<DaprClient>()));

            services.TryAddSingleton<IOutboxMessagesRepository>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>();
                var daprClient = sp.GetRequiredService<DaprClient>();
                var connectionManager = sp.GetRequiredService<IConnectionManager>();
                return new OutboxMessagesRepository(logger, daprClient, connectionManager);
            });

            return services;
        }
    }
}