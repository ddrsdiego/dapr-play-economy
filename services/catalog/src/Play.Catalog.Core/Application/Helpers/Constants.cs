namespace Play.Catalog.Core.Application.Helpers;

public static class Topics
{
    public const string CatalogItemCreated = "play-catalog.catalog-item-created";
    public const string CatalogItemUpdated = "play-catalog.catalog-item-updated";
}

public static class DaprParameters
{
    public const string PubSubName = "play-catalog-service-pubsub";
    public const string LockStoreName = "play-catalog-service-lock-store";
}