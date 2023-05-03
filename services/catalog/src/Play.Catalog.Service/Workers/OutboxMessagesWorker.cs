namespace Play.Catalog.Service.Workers;

using System.Threading;
using System.Threading.Tasks;
using Common.Application.Infra.Outbox;
using Microsoft.Extensions.Hosting;

internal sealed class OutboxMessagesWorker : BackgroundService
{
    private readonly IOutboxMessagesProcessor _outboxMessagesProcessor;

    public OutboxMessagesWorker(IOutboxMessagesProcessor outboxMessagesProcessor)
    {
        _outboxMessagesProcessor = outboxMessagesProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _outboxMessagesProcessor.RunAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _outboxMessagesProcessor.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}