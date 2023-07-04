namespace Play.Catalog.Core.Application.UseCases.CreateNewCatalogItem;

using Common.Application;
using Common.Application.Infra.Repositories.Dapr;
using Common.Application.Infra.UoW;
using Common.Application.Messaging.OutBox;
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
    private readonly IOutBoxMessagesRepository _outBoxMessagesRepository;

    public CreateNewCatalogItemCommandHandler(DaprClient daprClient,
        IDaprStateEntryRepository<CatalogItemData> daprStateEntryRepository,
        IOutBoxMessagesRepository outBoxMessagesRepository, IUnitOfWorkFactory unitOfWorkFactory)
    {
        _daprClient = daprClient;
        _daprStateEntryRepository = daprStateEntryRepository;
        _outBoxMessagesRepository = outBoxMessagesRepository;
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
            
        await using (var uow = await _unitOfWorkFactory.CreateAsync())
        {
            await uow.AddToContextAsync(async () => await _daprStateEntryRepository.UpsertAsync(catalogItemData, cancellationToken));
            await uow.AddToContextAsync(async () => await _outBoxMessagesRepository.SaveAsync(DaprParameters.PubSubName, eventName, topicName, catalogItemCreated, cancellationToken));
                
            await uow.SaveChangesAsync(cancellationToken);
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