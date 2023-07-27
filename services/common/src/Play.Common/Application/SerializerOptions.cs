namespace Play.Common.Application;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Helper to create instances of the JsonSerializerOptions class
/// </summary>
public sealed class SerializerOptions
{
    private bool _ignoreNullValues;
    private bool _propertyNameCaseInsensitive;
    private JsonNamingPolicy? _propertyNamingPolicy;

    /// <summary>
    /// Get a JsonSerializerOptions instance with IgnoreNullValues = true, PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    /// </summary>
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; private set; } = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Gets or sets a value that specifies the policy used to convert a property's name on an object to another format, such as camel-casing.
    /// </summary>
    /// <param name="jsonNamingPolicy"></param>
    /// <returns></returns>
    public SerializerOptions PropertyNamingPolicy(JsonNamingPolicy? jsonNamingPolicy)
    {
        _propertyNamingPolicy = jsonNamingPolicy;
        return this;
    }


    /// <summary>
    /// Gets or sets a value that determines whether a property's name uses a case-insensitive comparison during deserialization. The default value is false.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public SerializerOptions PropertyNameCaseInsensitive(bool value)
    {
        _propertyNameCaseInsensitive = value;
        return this;
    }

    /// <summary>
    /// Gets or sets a value that determines whether null values are ignored during serialization and deserialization. The default value is false.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public SerializerOptions IgnoreNullValues(bool value)
    {
        _ignoreNullValues = value;
        return this;
    }
}