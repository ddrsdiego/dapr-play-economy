namespace Play.Catalog.Core.Application.UseCases.CreateNewCatalogItem
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Dapr.Client;
    using Domain.AggregatesModel.CatalogItemAggregate;
    using Helpers;
    using Infra.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public sealed class CreateNewCatalogItemCommandHandler : IRequestHandler<CreateNewCatalogItemRequest, Response>
    {
        private readonly DaprClient _daprClient;
        private readonly IDaprStateEntryRepository<CatalogItemData> _daprStateEntryRepository;

        public CreateNewCatalogItemCommandHandler(DaprClient daprClient,
            IDaprStateEntryRepository<CatalogItemData> daprStateEntryRepository)
        {
            _daprClient = daprClient;
            _daprStateEntryRepository = daprStateEntryRepository;
        }

        public async Task<Response> Handle(CreateNewCatalogItemRequest request, CancellationToken cancellationToken)
        {
            var newCatalogItem = new CatalogItem(request.Price, request.Name, request.Description);

            var catalogItemData = newCatalogItem.ToCatalogItemData();
            await _daprStateEntryRepository.UpsertAsync(catalogItemData, cancellationToken);

            var catalogItemCreated = new CatalogItemCreated(newCatalogItem.Id, newCatalogItem.Descriptor.Name,
                newCatalogItem.Descriptor.Value);

            _ = _daprClient.PublishEventAsync(DaprParameters.PubSubName, Topics.CatalogItemCreated,
                catalogItemCreated,
                cancellationToken);

            var responseContent = new CreateNewCatalogItemResponse(
                newCatalogItem.Id,
                newCatalogItem.Descriptor.Name,
                newCatalogItem.Descriptor.Value,
                newCatalogItem.Price.Value,
                newCatalogItem.CreateAt
            );

            return Response.Ok(ResponseContent.Create(responseContent), StatusCodes.Status201Created);
        }
    }
}