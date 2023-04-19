namespace Play.Common.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    internal class ApiVersionFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parametersToRemove = operation.Parameters.Where(x => x.Name == "api-version").ToList();
            foreach (var parameter in parametersToRemove)
                operation.Parameters.Remove(parameter);
        }
    }
    
    public sealed class RequiredOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var swaggerHeaders = context.MethodInfo.GetCustomAttributes<RequiredHeaderAttribute>().Concat(
                context.MethodInfo.DeclaringType.GetCustomAttributes<RequiredHeaderAttribute>());

            foreach (var header in swaggerHeaders)
            {
                var openApiParameter = operation.Parameters.FirstOrDefault(p =>
                    p.In == ParameterLocation.Header && p.Name == header.HeaderName);

                if (openApiParameter is not null)
                    operation.Parameters.Remove(openApiParameter);

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = header.HeaderName,
                    In = ParameterLocation.Header,
                    Description = header.Description,
                    Required = header.IsRequired,
                    Schema = GetSchema(header.DefaultValue)
                });
            }
        }

        private static OpenApiSchema GetSchema(string headerDefaultValue)
        {
            var result = new OpenApiSchema {Type = "string"};

            if (!string.IsNullOrEmpty(headerDefaultValue))
                result.Default = new OpenApiString(headerDefaultValue);
            
            return result;
        }
    }
}