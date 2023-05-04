namespace Play.Catalog.Service;

using System;
using System.Data.Common;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Api;
using Common.Application.Infra.Repositories;
using Common.Application.Infra.UoW;
using Common.Application.Messaging;
using Common.Application.Messaging.InBox;
using Common.Application.Messaging.OutBox;
using Core.Application.Helpers;
using Core.Application.IoC;
using Core.Application.UseCases.CreateNewCatalogItem;
using Dapr.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Polly;
using Workers;

public abstract class Program
{
    public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilogCustom()
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<OutboxMessagesWorker>();
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssemblyContaining<CreateNewCatalogItemCommandHandler>());

                services.AddSwaggerVersioning();
                services.AddDaprStateEntryRepositories();
                services.AddDaprClient(configure => configure.UseJsonSerializationOptions(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                }));
                services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = false);
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
                        .GetSection("ConnectionStringOptions:PostgresConnection").Value;
                    return new ConnectionManager(npgsqlFactory, connectionString, resiliencePolicy);
                });

                services.TryAddSingleton<IUnitOfWorkFactory>(sp =>
                {
                    var connectionManager = sp.GetRequiredService<IConnectionManager>();
                    return new UnitOfWorkFactory(connectionManager);
                });

                services.TryAddSingleton<IOutBoxMessagesProcessor>(sp =>
                    new OutBoxMessagesProcessor(new BoxMessagesProcessorConfig
                        {
                            PubSubName = DaprParameters.PubSubName,
                            LockStoreName = DaprParameters.LockStoreName
                        },
                        sp.GetRequiredService<IOutBoxMessagesRepository>(),
                        sp.GetRequiredService<IUnitOfWorkFactory>(),
                        sp.GetRequiredService<DaprClient>()));

                services.TryAddSingleton<IInBoxMessagesRepository>(sp => new InBoxMessagesRepository(sp.GetRequiredService<IConnectionManager>()));
                services.TryAddSingleton<IOutBoxMessagesRepository>(sp => new OutBoxMessagesRepository(sp.GetRequiredService<IConnectionManager>()));
            });
}