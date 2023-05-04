namespace Play.Common.Api;

using System;

public enum MetricsApiTypeLevel
{
    Public,
    Private
}
    
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class MetricsApiTypeAttribute : Attribute
{
    public MetricsApiTypeAttribute(string operationId, MetricsApiTypeLevel level, string domain)
    {
        OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        Level = level;
        Domain = domain;
    }

    public string OperationId { get; }
    public MetricsApiTypeLevel Level { get; }
    public string Domain { get; }
}