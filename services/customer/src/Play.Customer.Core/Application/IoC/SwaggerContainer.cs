namespace Play.Customer.Core.Application.IoC;

using Microsoft.Extensions.DependencyInjection;

internal static class SwaggerContainer
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        // services.AddSwaggerGen(c =>
        //     c.SwaggerDoc("v1", new OpenApiInfo
        //     {
        //         Title = Assembly.GetEntryAssembly()?.GetName().Name
        //     }));
            
        return services;
    }
}