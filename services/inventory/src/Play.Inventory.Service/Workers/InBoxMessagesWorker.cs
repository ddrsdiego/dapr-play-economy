namespace Play.Inventory.Service.Workers;

using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging.InBox;
using Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Subscribers;
using Subscribers.Messages;

public sealed class InBoxMessagesWorker : BackgroundService
{
    private const string CustomerUpdatedChannelId = "play-customer.customer-updated-channel";
    private const string CatalogItemUpdatedChannelId = "play-catalog.catalog-item-updated-channel";
    private const string CatalogItemCreatedChannelId = "play-catalog.catalog-item-created-channel";

    private readonly IProcessorChannel<InBoxMessage> _customerNameUpdatedProcessorChannel;
    private readonly IProcessorChannel<InBoxMessage> _catalogItemCreatedProcessorChannel;
    private readonly IProcessorChannel<InBoxMessage> _catalogItemUpdatedProcessorChannel;
    private readonly IInBoxMessagesProcessor _inBoxMessagesProcessor;

    public InBoxMessagesWorker(IServiceScopeFactory serviceScopeFactory, IInBoxMessagesProcessor inBoxMessagesProcessor)
    {
        _inBoxMessagesProcessor = inBoxMessagesProcessor;

        _customerNameUpdatedProcessorChannel = ProcessorChannel<InBoxMessage>.Create(CustomerUpdatedChannelId, 200, 100, message => message.ExecuteCustomerNameUpdatedAsync(serviceScopeFactory));
        _catalogItemUpdatedProcessorChannel = ProcessorChannel<InBoxMessage>.Create(CatalogItemUpdatedChannelId, 200, 100, message => message.ExecuteCatalogItemUpdatedAsync(serviceScopeFactory));
        _catalogItemCreatedProcessorChannel = ProcessorChannel<InBoxMessage>.Create(CatalogItemCreatedChannelId, 200, 100, message => message.ExecuteCatalogItemCreatedAsync(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var inBoxMessage in _inBoxMessagesProcessor.GetUnprocessedMessagesAsync(stoppingToken))
        {
            switch (inBoxMessage.EventName)
            {
                case nameof(CustomerNameUpdated):
                    await _customerNameUpdatedProcessorChannel.EnqueueAsync(inBoxMessage, stoppingToken);
                    break;
                case nameof(CatalogItemCreated):
                    await _catalogItemCreatedProcessorChannel.EnqueueAsync(inBoxMessage, stoppingToken);
                    break;
                case nameof(CatalogItemUpdated):
                    await _catalogItemUpdatedProcessorChannel.EnqueueAsync(inBoxMessage, stoppingToken);
                    break;
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _catalogItemUpdatedProcessorChannel.DisposeAsync();
        await _catalogItemCreatedProcessorChannel.DisposeAsync();
        await _customerNameUpdatedProcessorChannel.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}