namespace Play.Inventory.Core.Application.IoC
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;

    internal static class SwaggerContainer
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = Assembly.GetEntryAssembly()?.GetName().Name
                }));
            
            return services;
        }
    }
}