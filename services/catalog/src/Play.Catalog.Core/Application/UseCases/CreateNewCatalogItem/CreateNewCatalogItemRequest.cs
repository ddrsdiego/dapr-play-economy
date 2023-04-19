namespace Play.Catalog.Core.Application.UseCases.CreateNewCatalogItem
{
    using System.Text.Json.Serialization;
    using Common.Application;
    using Common.Application.UseCase;
    using MediatR;

    public sealed class CreateNewCatalogItemRequest : UseCaseRequest, IRequest<Response>
    {
        [JsonConstructor]
        public CreateNewCatalogItemRequest(decimal price, string name, string description)
        {
            Price = price;
            Name = name;
            Description = description;
        }
        
        public decimal Price { get; }
        public string? Name { get; }
        public string? Description { get; }
    }
}