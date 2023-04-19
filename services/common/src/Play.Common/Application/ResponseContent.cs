namespace Play.Common.Application
{
    using System;
    using System.Buffers;
    using System.IO.Pipelines;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// An envelope for the content to be transported by the request
    /// </summary>
    public struct ResponseContent
    {
        private readonly object _sync;
        private Type InputType { get; }
        private string ContentAsJsonString { get; set; }
        private byte[] ContentAsJsonUtf8Bytes { get; set; }
        private JsonSerializerOptions JsonSerializerOptions { get; }

        private ResponseContent(byte[] contentAsJsonByte, string contentAsJsonString, Type inputType,
            SerializerOptions serializerOptions)
        {
            _sync = new object();
            InputType = inputType;
            ContentAsJsonUtf8Bytes = contentAsJsonByte;
            ContentAsJsonString = contentAsJsonString;
            JsonSerializerOptions = serializerOptions.GetOptions();
        }

        /// <summary>
        /// Encapsulates the content by passing the content type through generics
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentData"></param>
        /// <returns></returns>
        public static ResponseContent Create<TContent>(TContent contentData)
        {
            var serializerOptions = new SerializerOptions();
            serializerOptions
                .IgnoreNullValues(true)
                .PropertyNameCaseInsensitive(true)
                .PropertyNamingPolicy(JsonNamingPolicy.CamelCase);

            var contentAsJsonString = JsonSerializer.Serialize(contentData, serializerOptions.GetOptions());
            return new ResponseContent(null, contentAsJsonString, contentData.GetType(), serializerOptions);
        }

        /// <summary>
        /// Encapsulates the content by passing the content type through generics
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentData"></param>
        /// <param name="serializerOptions"></param>
        /// <returns></returns>
        public static ResponseContent Create<TContent>(TContent contentData, SerializerOptions serializerOptions = null)
        {
            var contentAsJsonString = serializerOptions == null
                ? JsonSerializer.Serialize(contentData)
                : JsonSerializer.Serialize(contentData, serializerOptions.GetOptions());

            return new ResponseContent(null, contentAsJsonString, contentData.GetType(), serializerOptions);
        }

        /// <summary>
        /// Encapsulates the content by passing the content type through string
        /// </summary>
        /// <param name="contentData"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public static ResponseContent Create(string contentData, Type inputType)
        {
            if (string.IsNullOrEmpty(contentData))
                throw new ArgumentNullException(nameof(contentData));

            var serializerOptions = new SerializerOptions();
            serializerOptions
                .IgnoreNullValues(true)
                .PropertyNameCaseInsensitive(true)
                .PropertyNamingPolicy(JsonNamingPolicy.CamelCase);

            return new ResponseContent(null, contentData, inputType, serializerOptions);
        }

        /// <summary>
        /// Encapsulates the content by passing the content type through Utf8 Bytes
        /// </summary>
        /// <param name="contentData"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public static ResponseContent Create(byte[] contentData, Type inputType)
        {
            if (contentData is null || contentData.Equals(default(byte[])))
                throw new ArgumentNullException(nameof(contentData));

            if (inputType is null)
                throw new ArgumentNullException(nameof(inputType));

            var serializerOptions = new SerializerOptions();
            serializerOptions
                .IgnoreNullValues(true)
                .PropertyNameCaseInsensitive(true)
                .PropertyNamingPolicy(JsonNamingPolicy.CamelCase);
            return new ResponseContent(contentData, string.Empty, inputType, serializerOptions);
        }

        /// <summary>
        /// Obtains the deserialized content using the type passed by parameter
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <returns></returns>
        public TContent GetRaw<TContent>() => (TContent) GetRaw(typeof(TContent));

        /// <summary>
        /// Obtains the deserialized content using the type passed by parameter
        /// </summary>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public object GetRaw(Type inputType)
        {
            var reader = new Utf8JsonReader(new ReadOnlySequence<byte>(ValueAsJsonUtf8Bytes));
            return JsonSerializer.Deserialize(ref reader, inputType, JsonSerializerOptions);
        }

        /// <summary>
        ///Get serialized content in Utf8 Bytes
        /// </summary>
        public byte[] ValueAsJsonUtf8Bytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_sync)
                {
                    if (ContentAsJsonUtf8Bytes is not null)
                        return ContentAsJsonUtf8Bytes;

                    var value = JsonSerializer.Deserialize(ValueAsJsonString, InputType, JsonSerializerOptions);
                    ContentAsJsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);

                    return ContentAsJsonUtf8Bytes;
                }
            }
        }

        /// <summary>
        ///Get serialized content in string
        /// </summary>
        public string ValueAsJsonString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (string.IsNullOrEmpty(ContentAsJsonString))
                {
                    ContentAsJsonString = JsonSerializer.Serialize(GetRaw(InputType), InputType, JsonSerializerOptions);
                }

                return ContentAsJsonString;
            }
        }

        public bool HasValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !string.IsNullOrEmpty(ContentAsJsonString) || ContentAsJsonUtf8Bytes?.Length > 0;
        }

        /// <summary>
        /// Write in Response.Body using PipeWriter by reference being serialized in UTF8
        /// </summary>
        /// <param name="pipe"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToPipe(PipeWriter pipe) => WriteToPipe(pipe, Encoding.UTF8.GetEncoder());

        /// <summary>
        /// Write in Response.Body using PipeWriter by reference
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="encoder"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToPipe(PipeWriter pipe, Encoder encoder)
        {
            Span<char> charSpan = stackalloc char[ValueAsJsonString.Length];
            for (int counter = 0; counter < ValueAsJsonString.Length; counter++)
            {
                charSpan[counter] = ValueAsJsonString[counter];
            }

            var bytesNeeded = encoder.GetByteCount(charSpan, true);
            var bytesWritten = encoder.GetBytes(charSpan, pipe.GetSpan(bytesNeeded), true);

            pipe.Advance(bytesWritten);
        }
    }
}