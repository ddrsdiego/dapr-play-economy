namespace Play.Inventory.Core.Application.IoC
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class OptionsContainer
    {
        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(options =>
                configuration.GetSection(nameof(AppSettings)).Bind(options));

            return services;
        }
    }
}