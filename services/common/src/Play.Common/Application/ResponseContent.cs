namespace Play.Common.Application
{
    using System;
    using System.IO.Pipelines;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// An envelope for the content to be transported by the request
    /// </summary>
    public struct ResponseContent
    {
        private static readonly SerializerOptions SerializerOptions;

        private readonly object _syncSerializeString;
        private readonly object _syncSerializeUtf8Bytes;
        private Type InputType { get; }
        private string ContentAsJsonString { get; set; }
        private ReadOnlyMemory<byte> ContentAsJsonUtf8Bytes { get; set; }

        private JsonSerializerOptions JsonSerializerOptions { get; }

        private ResponseContent(byte[] contentAsJsonByte, string contentAsJsonString, Type inputType,
            SerializerOptions serializerOptions)
        {
            _syncSerializeString = new object();
            _syncSerializeUtf8Bytes = new object();
            InputType = inputType;
            ContentAsJsonUtf8Bytes = contentAsJsonByte;
            ContentAsJsonString = contentAsJsonString;
            JsonSerializerOptions = serializerOptions.GetOptions();
        }

        static ResponseContent()
        {
            SerializerOptions = new SerializerOptions();
            SerializerOptions
                .IgnoreNullValues(true)
                .PropertyNameCaseInsensitive(true)
                .PropertyNamingPolicy(JsonNamingPolicy.CamelCase);
        }

        /// <summary>
        /// Encapsulates the content by passing the content type through generics
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentData"></param>
        /// <returns></returns>
        public static ResponseContent Create<TContent>(TContent contentData)
        {
            var contentAsUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(contentData, SerializerOptions.GetOptions());
            return new ResponseContent(contentAsUtf8Bytes, string.Empty, contentData.GetType(), SerializerOptions);
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
            var contentAsUtf8Bytes = serializerOptions == null
                ? JsonSerializer.SerializeToUtf8Bytes(contentData)
                : JsonSerializer.SerializeToUtf8Bytes(contentData, serializerOptions.GetOptions());

            return new ResponseContent(contentAsUtf8Bytes, string.Empty, contentData.GetType(), serializerOptions);
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

            return new ResponseContent(null, contentData, inputType, SerializerOptions);
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

            return new ResponseContent(contentData, string.Empty, inputType, SerializerOptions);
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
            var reader = new Utf8JsonReader(ValueAsJsonUtf8Bytes);
            return JsonSerializer.Deserialize(ref reader, inputType, JsonSerializerOptions);
        }

        /// <summary>
        ///Get serialized content in Utf8 Bytes
        /// </summary>
        public ReadOnlySpan<byte> ValueAsJsonUtf8Bytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_syncSerializeUtf8Bytes)
                {
                    if (ContentAsJsonUtf8Bytes.Length > 0) return ContentAsJsonUtf8Bytes.Span;

                    var value = JsonSerializer.Deserialize(ValueAsJsonString, InputType, JsonSerializerOptions);
                    ContentAsJsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);

                    return ContentAsJsonUtf8Bytes.Span;
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
                lock (_syncSerializeString)
                {
                    if (!string.IsNullOrEmpty(ContentAsJsonString)) return ContentAsJsonString;

                    ContentAsJsonString = JsonSerializer.Serialize(GetRaw(InputType), InputType, JsonSerializerOptions);
                    return ContentAsJsonString;
                }
            }
        }

        public bool HasValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !string.IsNullOrEmpty(ContentAsJsonString) || ContentAsJsonUtf8Bytes.Length > 0;
        }

        /// <summary>
        /// Write in Response.Body using PipeWriter by reference being serialized in UTF8
        /// </summary>
        /// <param name="pipe"></param>
        public void WriteToPipe(PipeWriter pipe) => WriteToPipe(pipe, Encoding.UTF8.GetEncoder());

        /// <summary>
        /// Write in Response.Body using PipeWriter by reference
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="encoder"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToPipe(PipeWriter pipe, Encoder encoder)
        {
            Span<char> charSpan = stackalloc char[ValueAsJsonUtf8Bytes.Length];
            for (var counter = 0; counter < ValueAsJsonUtf8Bytes.Length; counter++)
            {
                charSpan[counter] = (char) ValueAsJsonUtf8Bytes[counter];
            }

            var bytesNeeded = encoder.GetByteCount(charSpan, true);
            var bytesWritten = encoder.GetBytes(charSpan, pipe.GetSpan(bytesNeeded), true);

            pipe.Advance(bytesWritten);
        }
    }
}