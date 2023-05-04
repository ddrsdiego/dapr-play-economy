namespace Play.Common.Api;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequiredHeaderAttribute : Attribute
{
    public RequiredHeaderAttribute(string headerName, string description = null, string defaultValue = null,
        bool isRequired = true)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        Description = description;
        DefaultValue = defaultValue;
        IsRequired = isRequired;
    }

    public string HeaderName { get; }
    public string Description { get; }
    public string DefaultValue { get; }
    public bool IsRequired { get; }

    public static class NameConstants
    {
        public const string Auth = nameof(Auth);
    }
}