namespace Play.Inventory.Service
{
    using System.Buffers;
    using System.IO.Pipelines;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Messaging;
    using CSharpFunctionalExtensions;
    using Microsoft.AspNetCore.Http;

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
                    var messageEnvelope = JsonSerializer.Deserialize<MessageEnvelope>(buffer.FirstSpan,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    
                    eventToBeExtracted = JsonSerializer.Deserialize<T>(messageEnvelope.Body,
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
}