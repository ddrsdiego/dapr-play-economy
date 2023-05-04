namespace Play.Customer.Service.Subscribers.v1;

using System.Buffers;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Common.Application;
using Play.Customer.Core.Application.Helpers.Constants;
using Core.Domain.AggregateModel.CustomerAggregate;

internal static class CustomerCreatedSubscriber
{
    public static void HandleCustomerRegistered(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"/{Topics.CustomerRegistered}", async context =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(CustomerCreatedSubscriber));

            logger.LogInformation("");
                
            var newCustomerCreated = await context.ReadFromBodyAsync<NewCustomerCreated>();
            if (newCustomerCreated.IsFailure)
            {
                var error = new Error("", "");
                var response = Response.Fail(error);
                await response.WriteToPipeAsync(context.Response);
            }
        }).WithTopic(DaprSettings.PubSubName, Topics.CustomerRegistered);
    }
}

internal static class PipeReaderEx
{
    public static ValueTask<Result<T>> ReadFromBodyAsync<T>(this HttpContext context) =>
        context.Request.BodyReader.ReadFromBodyAsync<T>(context.RequestAborted);

    private static async ValueTask<Result<T>> ReadFromBodyAsync<T>(this PipeReader reader,
        CancellationToken cancellationToken)
    {
        T eventToBeExtracted = default;
        while (!cancellationToken.IsCancellationRequested)
        {
            var readResult = await reader.ReadAsync(cancellationToken);
            var buffer = readResult.Buffer;
            var position = buffer.PositionOf((byte) '}');

            if (position != null)
            {
                eventToBeExtracted = JsonSerializer.Deserialize<T>(buffer.FirstSpan,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (readResult.IsCompleted) break;
        }

        return eventToBeExtracted;
    }
}