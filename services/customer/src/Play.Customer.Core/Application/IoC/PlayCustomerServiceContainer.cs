namespace Play.Customer.Core.Application.IoC
{
    using Common.Application.Infra.Outbox;
    using Common.Application.Infra.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using UseCases.RegisterNewCustomer;

    public static class PlayCustomerServiceContainer
    {
        public static IServiceCollection AddPlayCustomerServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions(configuration);
            services.AddSwagger();
            services.AddDaprClient();
            services.AddRepositories(configuration);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterNewCustomerCommandHandler>());
            services.TryAddSingleton<IOutboxMessagesRepository>(sp => new OutboxMessagesRepository(sp.GetRequiredService<ILoggerFactory>(),
                sp.GetRequiredService<IConnectionManager>()));
            return services;
        }
    }
}