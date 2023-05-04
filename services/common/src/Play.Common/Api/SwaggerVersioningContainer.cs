namespace Play.Common.Api;

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

public static class SwaggerVersioningContainer
{
    public static IServiceCollection AddSwaggerVersioning(this IServiceCollection services, string title = null)
    {
        title ??= Assembly.GetEntryAssembly()?.GetName().Name;

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddRouting(c => c.LowercaseUrls = true);
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.DescribeAllParametersInCamelCase();
        });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>(sp =>
            new ConfigureSwaggerGenOptions(sp.GetRequiredService<IApiVersionDescriptionProvider>(), title));

        return services;
    }

    public static IApplicationBuilder UseSwaggerVersioning(this IApplicationBuilder app,
        IApiVersionDescriptionProvider apiVersionProvider = null, bool customServerSwagger = false)
    {
        apiVersionProvider ??= app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger(customServerSwagger ? ConfigureSwagger.TryGetOpenApiServer() : null);
        app.UseSwaggerUI(c =>
        {
            foreach (var description in apiVersionProvider.ApiVersionDescriptions.Select(x => x.GroupName))
                c.SwaggerEndpoint($"/swagger/{description}/swagger.json", description.ToLowerInvariant());
        });

        return app;
    }
}