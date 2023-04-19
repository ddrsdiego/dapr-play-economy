namespace Play.Inventory.Core.Application.Helpers.Constants
{
    public static class DaprSettings
    {
        public static class PubSub
        {
            public const string Name = "play-inventory-pub-sub";          
        }
    }
    
    public static class Topics
    {
        public const string CustomerUpdated = "play-customer.customer-updated";
        public const string CustomerRegistered = "play-customer.customer-created";
        public const string CatalogItemCreated = "play-catalog.catalog-item-created";
        public const string CatalogItemUpdated = "play-catalog.catalog-item-updated";
    }
}