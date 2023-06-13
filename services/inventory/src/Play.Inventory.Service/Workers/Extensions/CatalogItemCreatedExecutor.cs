namespace Play.Inventory.Service.Workers.Extensions;

using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging.InBox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Subscribers;

internal static class CatalogItemCreatedExecutor
{
    public static async Task ExecuteCatalogItemCreatedAsync(this InBoxMessage message, IServiceScopeFactory serviceScopeFactory)
    {
        var customerNameUpdatedResult = message.AdapterFromInBoxMessage<CatalogItemCreated>();

        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var inBoxMessagesProcessor = scope.ServiceProvider.GetRequiredService<IInBoxMessagesProcessor>();

        var response = await mediator.Send(customerNameUpdatedResult.Value.ToCommand());
        if (response.IsSuccess)
            await inBoxMessagesProcessor.MarkMessageAsProcessedAsync(message, CancellationToken.None);
        else
            await inBoxMessagesProcessor.MarkMessageAsFailedAsync(message, CancellationToken.None);
    }
}