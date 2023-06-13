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

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var inBoxMessagesProcessor = scope.ServiceProvider.GetRequiredService<IInBoxMessagesProcessor>();
        
        try
        {
            var customerNameUpdatedResult = message.AdapterFromInBoxMessage<CustomerNameUpdated>();

            var response = await mediator.Send(customerNameUpdatedResult.Value.ToCommand());
            if (response.IsSuccess)
                await inBoxMessagesProcessor.MarkMessageAsProcessedAsync(message, CancellationToken.None);
            else
                await inBoxMessagesProcessor.MarkMessageAsFailedAsync(message, CancellationToken.None);
        }
        catch (Exception e)
        {
            await inBoxMessagesProcessor.MarkMessageAsFailedAsync(message, CancellationToken.None);
        }
    }
}