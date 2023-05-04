namespace Play.Inventory.Core.Application.UseCases.CreateCatalogItem;

using Common.Application;
using MediatR;

public record struct CreateCatalogItemCommand(string CatalogItemId, string Name, string Description, decimal UnitPrice) : IRequest<Response>;