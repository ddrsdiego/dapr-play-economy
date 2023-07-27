namespace Play.Common.Application.Messaging;

using InBox;
using InBox.Observers.ProcessorExecute;
using Infra.Repositories;
using Infra.UoW;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class MessagingContainer
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingSettings = configuration.GetSection(nameof(MessagingSettings)).Get<MessagingSettings>() ?? new MessagingSettings
        {
            InBox = InBoxMessagingSettings.Default
        };

        services.AddSingleton(messagingSettings);

        services.AddSingleton<IInBoxMessageReceiver, InBoxMessageReceiver>();
        services.AddSingleton<IInBoxMessageBatchProcessor, InBoxMessageBatchProcessor>();

        services.AddSingleton<IInBoxMessageProcessor>(sp =>
        {
            var unitOfWorkFactory = sp.GetRequiredService<IUnitOfWorkFactory>();
            var inBoxMessageRepository = sp.GetRequiredService<IInBoxMessageStorage>();
            var inBoxMessageReceiver = sp.GetRequiredService<IInBoxMessageReceiver>();
            var logger = sp.GetRequiredService<ILogger<InBoxMessageProcessor>>();

            var inBoxMessageProcessor = new InBoxMessageProcessor(unitOfWorkFactory, inBoxMessageRepository, inBoxMessageReceiver);
            inBoxMessageProcessor.ConnectInBoxMessageProcessorExecuteObserver(new LogInBoxMessageProcessorObserver(logger));

            return inBoxMessageProcessor;
        });

        services.TryAddSingleton<IInBoxMessageStorageRead>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>();
            var connectionManager = sp.GetRequiredService<IConnectionManagerFactory>();
            var settings = sp.GetRequiredService<MessagingSettings>();
            var applicationLifetime = sp.GetRequiredService<IHostApplicationLifetime>();
            return new InBoxMessagePostgresStorageRead(logger, settings, connectionManager);
        });

        services.TryAddSingleton<IInBoxMessageStorageWrite>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>();
            var connectionManager = sp.GetRequiredService<IConnectionManagerFactory>();
            var applicationLifetime = sp.GetRequiredService<IHostApplicationLifetime>();
            return new InBoxMessagePostgresStorageWrite(logger, connectionManager);
        });

        services.TryAddSingleton<IInBoxMessageStorage>(sp => new InBoxMessagePostgresStorage(
            sp.GetRequiredService<IInBoxMessageStorageRead>(),
            sp.GetRequiredService<IInBoxMessageStorageWrite>()));

        return services;
    }
}