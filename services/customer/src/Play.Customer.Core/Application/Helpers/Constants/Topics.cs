namespace Play.Customer.Core.Application.Helpers.Constants
{
    public static class DaprSettings
    {
        public static class PubSub
        {
            public const string Name = "play-customer-pub-sub";          
        }
    }
    
    public static class Topics
    {
        public const string CustomerUpdated = "play-customer.customer-updated";
        public const string CustomerRegistered = "play-customer.customer-created";
    }
}