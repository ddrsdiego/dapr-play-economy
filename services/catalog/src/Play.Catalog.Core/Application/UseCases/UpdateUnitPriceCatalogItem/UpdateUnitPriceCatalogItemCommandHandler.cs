namespace Play.Catalog.Core.Application.UseCases.UpdateUnitPriceCatalogItem
{
    using Common.Application;
    using Common.Application.Infra.Repositories.Dapr;
    using Dapr.Client;
    using Domain.AggregatesModel.CatalogItemAggregate;
    using Microsoft.Extensions.Logging;
    using Helpers;
    using Infra.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    internal sealed class UpdateUnitPriceCatalogItemCommandHandler : IRequestHandler<UpdateUnitPriceCatalogItemCommand, Response>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<UpdateUnitPriceCatalogItemCommandHandler> _logger;
        private readonly IDaprStateEntryRepository<CatalogItemData> _daprStateEntryRepository;

        private const string ItemNotFoundError = "ITEM_NOT_FOUND";

        public UpdateUnitPriceCatalogItemCommandHandler(ILogger<UpdateUnitPriceCatalogItemCommandHandler> logger,
            DaprClient daprClient,
            IDaprStateEntryRepository<CatalogItemData> daprStateEntryRepository)
        {
            _logger = logger;
            _daprClient = daprClient;
            _daprStateEntryRepository = daprStateEntryRepository;
        }

        public async Task<Response> Handle(UpdateUnitPriceCatalogItemCommand request,
            CancellationToken cancellationToken)
        {
            var stateEntry =
                await _daprStateEntryRepository.GetCustomerByIdAsync(request.CatalogItemId, cancellationToken);
            
            if (stateEntry.IsFailure)
            {
                return Response.Fail(new Error(ItemNotFoundError,
                    $"Item not found in catalog with id {request.CatalogItemId}"));
            }

            var catalogItem = stateEntry.Value.ToCatalogItem();
            var newUnitPrice = new UnitPrice(request.UnitPrice);

            catalogItem.UpdateUnitePrice(newUnitPrice);

            var data = catalogItem.ToCatalogItemData();
            await _daprStateEntryRepository.UpsertAsync(data, cancellationToken);

            var catalogItemUpdated = new CatalogItemUpdated(catalogItem.Id, catalogItem.Descriptor.Name,
                catalogItem.Descriptor.Value,
                catalogItem.Price.Value);

            _daprClient.PublishEventAsync(DaprParameters.PubSubName, Topics.CatalogItemUpdated,
                catalogItemUpdated,
                cancellationToken).FireAndForget();
            
            _ = _daprClient.PublishEventAsync(DaprParameters.PubSubName, Topics.CatalogItemUpdated,
                catalogItemUpdated,
                cancellationToken);

            return Response.Ok(StatusCodes.Status200OK);
        }
    }

    internal static class TaskEx
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x =>
            {
                if (x.IsCanceled)
                {
                }
            });
        }
    }
}