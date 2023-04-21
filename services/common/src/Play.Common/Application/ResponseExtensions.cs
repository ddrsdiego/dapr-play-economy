namespace Play.Common.Application
{
    using System;
    using System.IO.Pipelines;
    using System.Net.Mime;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public static class ReadExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskReadResponse"></param>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        public static async ValueTask WriteToPipeAsync(this ValueTask<Response> taskReadResponse,
            HttpResponse response, CancellationToken cancellationToken = default)
        {
            var readResponse = await taskReadResponse;
            await readResponse.WriteToPipeAsync(response, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskReadResponse"></param>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        public static async ValueTask WriteToPipeAsync(this Task<Response> taskReadResponse, HttpResponse response,
            CancellationToken cancellationToken = default)
        {
            var readResponse = await taskReadResponse;
            await readResponse.WriteToPipeAsync(response, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readResponse"></param>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ValueTask WriteToPipeAsync(this Response readResponse, HttpResponse response,
            CancellationToken cancellationToken = default)
        {
            response.StatusCode = readResponse.StatusCode;
            response.ContentType = MediaTypeNames.Application.Json;

            await response.StartAsync(cancellationToken);

            if (readResponse.HasBodyToWrite)
                response.BodyWriter.WriteToPipe(readResponse.Content.ValueAsJsonUtf8Bytes);
            
            await response.BodyWriter.FlushAsync(cancellationToken);
            await response.BodyWriter.CompleteAsync();
         }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        private static void WriteToPipe(this PipeWriter writer, ReadOnlySpan<byte> value) =>
            writer.WriteToPipe(value, Encoding.UTF8.GetEncoder());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="encoder"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteToPipe(this PipeWriter writer, ReadOnlySpan<byte> value, Encoder encoder)
        {
            Span<char> charSpan = stackalloc char[value.Length];
            for (var counter = 0; counter < value.Length; counter++)
            {
                charSpan[counter] = (char) value[counter];
            }

            var bytesNeeded = encoder.GetByteCount(charSpan, true);
            var bytesWritten = encoder.GetBytes(charSpan, writer.GetSpan(bytesNeeded), true);

            // Advance the PipeWriter to indicate how much data was written.
            writer.Advance(bytesWritten);
        }
    }
}