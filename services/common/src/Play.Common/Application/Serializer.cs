namespace Play.Common.Application;

using System;
using System.Text;
using System.Text.Json;

public static class Serializer
{
    /// <summary>
    /// Converts a given object to its JSON representation.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string ToJson<T>(ref T value) => ToJson(ref value, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given object to its JSON representation using provided serialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string ToJson<T>(ref T value, JsonSerializerOptions options) => JsonSerializer.Serialize(value, options);

    /// <summary>
    /// Converts a given object to its JSON representation.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="type">The type of the object.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string ToJson(ref string value, Type type) => ToJson(ref value, type, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given object to its JSON representation using provided serialization options.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="type">The type of the object.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string ToJson(ref string value, Type type, JsonSerializerOptions options) => JsonSerializer.Serialize(value, type, options);

    /// <summary>
    /// Converts a given object to its UTF8 bytes representation.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The UTF8 bytes representation of the object.</returns>
    public static byte[] ToUtf8Bytes<T>(ref T value) => ToUtf8Bytes(ref value, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given object to its UTF8 bytes representation using provided serialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The UTF8 bytes representation of the object.</returns>
    public static byte[] ToUtf8Bytes<T>(ref T value, JsonSerializerOptions options) => JsonSerializer.SerializeToUtf8Bytes(value, options);

    /// <summary>
    /// Converts a given JSON content (byte array) to an object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The byte array to deserialize.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(byte[] content) => FromJson<T>(content, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given JSON content (byte array) to an object using provided deserialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The byte array to deserialize.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(byte[] content, JsonSerializerOptions options)
    {
        var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(content));
        return JsonSerializer.Deserialize<T>(ref reader, options)!;
    }

    /// <summary>
    /// Converts a given JSON content (byte array) to an object using provided deserialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The byte array to deserialize.</param>
    /// <param name="type">The type of the object.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(byte[] content, Type type) => FromJson<T>(content, type, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given JSON content (byte array) to an object using provided deserialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The byte array to deserialize.</param>
    /// <param name="type">The type of the object.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(byte[] content, Type type, JsonSerializerOptions options)
    {
        var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(content));
        return (T)JsonSerializer.Deserialize(ref reader, type, options)!;
    }

    /// <summary>
    /// Converts a given JSON content (ReadOnlySpan byte array) to an object of a specified type.
    /// </summary>
    /// <param name="content">The ReadOnlySpan byte array to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static object FromJson(ReadOnlySpan<byte> content, Type type) => FromJson(content, type, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given JSON content (ReadOnlySpan byte array) to an object of a specified type using provided deserialization options.
    /// </summary>
    /// <param name="content">The ReadOnlySpan byte array to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static object FromJson(ReadOnlySpan<byte> content, Type type, JsonSerializerOptions options)
    {
        var reader = new Utf8JsonReader(content);
        return JsonSerializer.Deserialize(ref reader, type, options)!;
    }

    /// <summary>
    /// Converts a given JSON content (string) to an object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The string to deserialize.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(string content) => FromJson<T>(content, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given JSON content (string) to an object using provided deserialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The string to deserialize.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(string content, JsonSerializerOptions options) => FromJson<T>(Encoding.UTF8.GetBytes(content), options);

    /// <summary>
    /// Converts a given JSON content (string) to an object using provided deserialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The string to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(string content, Type type) => FromJson<T>(content, type, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given JSON content (string) to an object of a specified type using provided deserialization options.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="content">The string to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static T FromJson<T>(string content, Type type, JsonSerializerOptions options) => FromJson<T>(Encoding.UTF8.GetBytes(content), type, options);

    /// <summary>
    /// Converts a given JSON content (string) to an object of a specified type.
    /// </summary>
    /// <param name="content">The string to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static object FromJson(string content, Type type) => FromJson(content, type, SerializerOptions.DefaultJsonSerializerOptions);

    /// <summary>
    /// Converts a given JSON content (string) to an object of a specified type using provided deserialization options.
    /// </summary>
    /// <param name="content">The string to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The deserialized object from the JSON content.</returns>
    public static object FromJson(string content, Type type, JsonSerializerOptions options) => FromJson(Encoding.UTF8.GetBytes(content), type, options);
}