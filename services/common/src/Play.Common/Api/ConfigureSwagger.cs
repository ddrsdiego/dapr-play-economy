namespace Play.Common.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Swagger;

    public static class ConfigureSwagger
    {
        public static void UseSwagger(this IApplicationBuilder app, string swaggerJsonUrl, string swaggerName,
            bool customerServerSwagger = false)
        {
            app.UseSwagger(customerServerSwagger ? TryGetOpenApiServer() : null);
        }

        public static Action<SwaggerOptions> TryGetOpenApiServer()
        {
            return options =>
            {
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var server = httpReq.Headers["Referer"].FirstOrDefault()?.Split("swagger")[0];
                    if (!string.IsNullOrEmpty(server))
                    {
                        swaggerDoc.Servers = new List<OpenApiServer>
                        {
                            new() {Url = server}
                        };
                    }
                });
            };
        }
    }
}