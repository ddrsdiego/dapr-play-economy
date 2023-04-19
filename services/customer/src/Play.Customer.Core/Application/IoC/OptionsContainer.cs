namespace Play.Customer.Core.Application.IoC
{
    using Infra.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class OptionsContainer
    {
        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(options => configuration.GetSection(nameof(AppSettings)).Bind(options));
            services.Configure<ConnectionStringOptions>(options => configuration.GetSection(nameof(ConnectionStringOptions)).Bind(options));
            return services;
        }
    }
}