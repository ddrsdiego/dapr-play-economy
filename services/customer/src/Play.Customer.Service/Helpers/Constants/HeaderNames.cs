namespace Play.Customer.Service.Helpers.Constants;

public static class HeaderNames
{
    public const string XRequestId = "X-Request-ID";
}

public static class HeaderDescriptions
{
    public const string XRequestId = "Include a unique HTTP X-Request-ID request header to ensure idempotent message processing in case of a retry.";
}