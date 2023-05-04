namespace Play.Common.Api;

using Microsoft.Extensions.DependencyInjection;

public static class AppBaseConfigContainer
{
    public static IServiceCollection AddAppBaseConfig(this IServiceCollection services)
    {
        services.AddSwaggerVersioning();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<ApiVersionFilter>();
            options.OperationFilter<RequiredOperationFilter>();
        });
        return services;
    }
}