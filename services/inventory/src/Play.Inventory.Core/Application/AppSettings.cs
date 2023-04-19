namespace Play.Inventory.Core.Application
{
    public class AppSettings
    {
        public DaprSettings DaprSettings { get; set; }
        
    }

    public class DaprSettings
    {
        public string PubSubName { get; set; }
        public string StateStoreName { get; set; }
    }
}