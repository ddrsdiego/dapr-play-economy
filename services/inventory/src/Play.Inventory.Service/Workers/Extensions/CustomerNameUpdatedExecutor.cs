namespace Play.Inventory.Service.Workers.Extensions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging.InBox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Subscribers.Messages;

internal static class CustomerNameUpdatedExecutor
{
    public static async Task ExecuteCustomerNameUpdatedAsync(this InBoxMessage message, IServiceScopeFactory serviceScopeFactory)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var customerNameUpdatedResult = message.AdapterFromInBoxMessage<CustomerNameUpdated>();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var inBoxMessagesProcessor = scope.ServiceProvider.GetRequiredService<IInBoxMessageProcessor>();

        await inBoxMessagesProcessor.ExecuteAsync<CustomerNameUpdated>(message, async (_, _) =>
            await mediator.Send(customerNameUpdatedResult.Value.ToCommand()), CancellationToken.None);
    }
}