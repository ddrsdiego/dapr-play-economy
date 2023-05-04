namespace Play.Common.Application;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Helper to create instances of the JsonSerializerOptions class
/// </summary>
public sealed class SerializerOptions
{
    private static JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Get a JsonSerializerOptions instance with IgnoreNullValues = true, PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    /// </summary>
    public static JsonSerializerOptions GetOptionsDefault()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return _jsonSerializerOptions;
    }

    private JsonNamingPolicy _propertyNamingPolicy;

    /// <summary>
    /// Gets or sets a value that specifies the policy used to convert a property's name on an object to another format, such as camel-casing.
    /// </summary>
    /// <param name="jsonNamingPolicy"></param>
    /// <returns></returns>
    public SerializerOptions PropertyNamingPolicy(JsonNamingPolicy jsonNamingPolicy)
    {
        _propertyNamingPolicy = jsonNamingPolicy;
        return this;
    }

    private bool _propertyNameCaseInsensitive;

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

    private bool _ignoreNullValues;

    /// <summary>
    ///Gets or sets a value that determines whether null values are ignored during serialization and deserialization. The default value is false.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public SerializerOptions IgnoreNullValues(bool value)
    {
        _ignoreNullValues = value;
        return this;
    }

    /// <summary>
    /// Creates a JsonSerializerOptions instance with user-defined parameters
    /// </summary>
    /// <returns></returns>
    public JsonSerializerOptions GetOptions()
    {
        return _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = _propertyNamingPolicy,
            PropertyNameCaseInsensitive = _propertyNameCaseInsensitive,
        };
    }
}