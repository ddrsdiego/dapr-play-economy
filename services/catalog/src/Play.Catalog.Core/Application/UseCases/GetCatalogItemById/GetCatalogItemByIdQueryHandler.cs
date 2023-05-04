namespace Play.Catalog.Core.Application.UseCases.GetCatalogItemById;

using Common.Application;
using Common.Application.Infra.Repositories.Dapr;
using Infra.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class GetCatalogItemByIdQueryHandler : IRequestHandler<GetCatalogItemByIdRequest, Response>
{
    private const string ItemNotFoundError = "ITEM_NOT_FOUND";

    private readonly ILogger<GetCatalogItemByIdQueryHandler> _logger;
    private readonly IDaprStateEntryRepository<CatalogItemData> _entryRepository;

    public GetCatalogItemByIdQueryHandler(ILogger<GetCatalogItemByIdQueryHandler> logger,
        IDaprStateEntryRepository<CatalogItemData> entryRepository)
    {
        _logger = logger;
        _entryRepository = entryRepository;
    }

    public async Task<Response> Handle(GetCatalogItemByIdRequest request, CancellationToken cancellationToken)
    {
        var catalogItemData = await _entryRepository.GetCustomerByIdAsync(request.Id, cancellationToken);
        if (catalogItemData.IsFailure)
            return Response.Fail(new Error(ItemNotFoundError, $"Item not found in catalog with id {request.Id}"));

        var response = new GetCatalogItemByIdResponse(
            catalogItemData.Value.CatalogItemId,
            catalogItemData.Value.CatalogItemName,
            catalogItemData.Value.Description,
            catalogItemData.Value.Price,
            catalogItemData.Value.CreateAt
        );

        return Response.Ok(ResponseContent.Create(response));
    }
}