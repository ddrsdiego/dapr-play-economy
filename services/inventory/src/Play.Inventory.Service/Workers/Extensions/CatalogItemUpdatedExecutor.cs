namespace Play.Inventory.Service.Workers.Extensions;

using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging.InBox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Subscribers;

internal static class CatalogItemUpdatedExecutor
{
    public static async Task ExecuteCatalogItemUpdatedAsync(this InBoxMessage message, IServiceScopeFactory serviceScopeFactory)
    {
        var customerNameUpdatedResult = message.AdapterFromInBoxMessage<CatalogItemUpdated>();

        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var inBoxMessagesProcessor = scope.ServiceProvider.GetRequiredService<IInBoxMessageProcessor>();

        await inBoxMessagesProcessor.ExecuteAsync<CatalogItemUpdated>(message, async (_, _) =>
            await mediator.Send(customerNameUpdatedResult.Value.ToCommand()), CancellationToken.None);
    }
}