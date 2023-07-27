namespace Play.Inventory.Service.Workers.Extensions;

using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging.InBox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Subscribers;

internal static class CatalogItemCreatedExecutor
{
    public static async Task ExecuteCatalogItemCreatedAsync(this InBoxMessage inBoxMessage, IServiceScopeFactory serviceScopeFactory)
    {
        var customerNameUpdatedResult = inBoxMessage.AdapterFromInBoxMessage<CatalogItemCreated>();

        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var inBoxMessagesProcessor = scope.ServiceProvider.GetRequiredService<IInBoxMessageProcessor>();

        await inBoxMessagesProcessor.ExecuteAsync<CatalogItemCreated>(inBoxMessage, async (_, _) =>
            await mediator.Send(customerNameUpdatedResult.Value.ToCommand()), CancellationToken.None);
    }
}