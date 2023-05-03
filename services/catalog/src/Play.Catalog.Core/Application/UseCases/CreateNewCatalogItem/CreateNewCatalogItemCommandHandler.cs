namespace Play.Catalog.Core.Application.UseCases.CreateNewCatalogItem
{
    using Common.Application;
    using Common.Application.Infra;
    using Common.Application.Infra.Outbox;
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
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IOutboxMessagesRepository _outboxMessagesRepository;

        public CreateNewCatalogItemCommandHandler(DaprClient daprClient,
            IDaprStateEntryRepository<CatalogItemData> daprStateEntryRepository,
            IOutboxMessagesRepository outboxMessagesRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _daprClient = daprClient;
            _daprStateEntryRepository = daprStateEntryRepository;
            _outboxMessagesRepository = outboxMessagesRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task<Response> Handle(CreateNewCatalogItemRequest request, CancellationToken cancellationToken)
        {
            var newCatalogItem = new CatalogItem(request.Price, request.Name, request.Description);

            var catalogItemData = newCatalogItem.ToCatalogItemData();

            const string eventName = nameof(CatalogItemCreated);
            const string topicName = Topics.CatalogItemCreated;
            
            var catalogItemCreated = new CatalogItemCreated(newCatalogItem.Id, newCatalogItem.Descriptor.Name,
                newCatalogItem.Descriptor.Value);
            
            await using (var uow = await _unitOfWorkFactory.CreateAsync(cancellationToken))
            {
                uow.AddToContext(async () => await _daprStateEntryRepository.UpsertAsync(catalogItemData, cancellationToken));
                uow.AddToContext(async () => await _outboxMessagesRepository.SaveAsync(DaprParameters.PubSubName, eventName, topicName, catalogItemCreated, cancellationToken));
                
                await uow.SaveChangesAsync();
            }

            var responseContent = new CreateNewCatalogItemResponse(newCatalogItem.Id,
                newCatalogItem.Descriptor.Name,
                newCatalogItem.Descriptor.Value,
                newCatalogItem.Price.Value,
                newCatalogItem.CreateAt
            );

            return Response.Ok(ResponseContent.Create(responseContent), StatusCodes.Status201Created);
        }
    }
}