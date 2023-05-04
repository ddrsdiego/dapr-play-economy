namespace Play.Customer.Core.Application.Helpers.Constants;

public static class DaprSettings
{
    public const string PubSubName = "play-customer-service-pubsub";
    public const string StateStoreName = "play-customer-service-state-store";
}

public static class Topics
{
    public const string CustomerUpdated = "play-customer.customer-updated";
    public const string CustomerRegistered = "play-customer.customer-created";
}