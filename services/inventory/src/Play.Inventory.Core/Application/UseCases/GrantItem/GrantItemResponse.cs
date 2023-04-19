namespace Play.Inventory.Core.Application.UseCases.GrantItem
{
    using System.Text.Json.Serialization;

    public readonly struct GrantItemResponse
    {
        [JsonConstructor]
        public GrantItemResponse(string userId)
        {
            UserId = userId;
        }
        
        public string UserId { get; }
    }
}