namespace Play.Customer.Service.Workers;

using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging.OutBox;
using Microsoft.Extensions.Hosting;

internal sealed class OutboxMessagesWorker : BackgroundService
{
    private readonly IOutBoxMessagesProcessor _outBoxMessagesProcessor;

    public OutboxMessagesWorker(IOutBoxMessagesProcessor outBoxMessagesProcessor) => _outBoxMessagesProcessor = outBoxMessagesProcessor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await _outBoxMessagesProcessor.RunAsync(stoppingToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _outBoxMessagesProcessor.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}