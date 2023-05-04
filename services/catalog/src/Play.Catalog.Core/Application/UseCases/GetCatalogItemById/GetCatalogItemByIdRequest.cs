namespace Play.Catalog.Core.Application.UseCases.GetCatalogItemById;

using Common.Application;
using Common.Application.UseCase;
using MediatR;

public sealed class GetCatalogItemByIdRequest : UseCaseRequest, IRequest<Response>
{
    public GetCatalogItemByIdRequest(string id) => Id = id;

    public string Id { get; }
}