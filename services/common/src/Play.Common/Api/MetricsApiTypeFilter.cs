namespace Play.Common.Api
{
    using System.Linq;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Extensions;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public sealed class MetricsApiTypeFilter : IOperationFilter
    {
        private const string MetricsTag = "x-metrics";
        private const string OperationTag = "x-operation-id";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<MetricsApiTypeAttribute>()
                .SingleOrDefault();

            if (attr is null)
                return;

            operation.AddExtension(MetricsTag,
                new OpenApiObject
                {
                    ["level"] = new OpenApiString(attr.Level.GetDisplayName().ToLowerInvariant()),
                    ["domain"] = new OpenApiString(attr.Domain)
                });
        }
    }
}