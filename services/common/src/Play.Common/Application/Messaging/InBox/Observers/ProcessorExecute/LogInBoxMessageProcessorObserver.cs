namespace Play.Common.Application.Messaging.InBox.Observers.ProcessorExecute;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Play.Common.Application.Infra.UoW.Observers.SaveChanges;
using Play.Common.Application.Messaging.InBox;

internal sealed class LogInBoxMessageProcessorObserver :
    IInBoxMessageProcessorObserver
{
    private readonly ILogger _logger;
    private const string InBoxMessageProcessorMessage = "in-box-message-processor";

    public LogInBoxMessageProcessorObserver(ILogger logger) => _logger = logger;

    public Task OnPreProcess(InBoxMessage inBoxMessage)
    {
        inBoxMessage.StartProcessorExecuteWatch();
        var context = InBoxMessageLogContextFactory.Create(inBoxMessage);

        _logger.LogDebug($"[{LogFields.LogType}] - {LogFields.StepProcess} - {LogFields.InBoxMessageLogContext}",
            InBoxMessageProcessorMessage,
            "OnPreProcess",
            context);

        return Task.CompletedTask;
    }

    public Task OnPostProcess(InBoxMessage inBoxMessage)
    {
        inBoxMessage.StopProcessorExecuteWatch();
        var context = InBoxMessageLogContextFactory.Create(inBoxMessage);

        _logger.LogInformation($"[{LogFields.LogType}] - {LogFields.StepProcess} - {LogFields.InBoxMessageLogContext}",
            InBoxMessageProcessorMessage,
            "OnPostProcess",
            context);

        return Task.CompletedTask;
    }

    public Task OnProcessError(InBoxMessage inBoxMessage, Exception exception)
    {
        inBoxMessage.StopProcessorExecuteWatch();
        var context = InBoxMessageLogContextFactory.Create(inBoxMessage);

        _logger.LogError(exception, $"[{LogFields.LogType}] - {LogFields.StepProcess} - {LogFields.InBoxMessageLogContext}",
            InBoxMessageProcessorMessage,
            "OnProcessError",
            context);

        return Task.CompletedTask;
    }

    private static class InBoxMessageLogContextFactory
    {
        public static InBoxMessageLogContext Create(InBoxMessage inBoxMessage) => new(inBoxMessage);
    }
}

internal sealed class InBoxMessageLogContext
{
    private readonly InBoxMessage _inBoxMessage;

    public InBoxMessageLogContext(InBoxMessage inBoxMessage) => _inBoxMessage = inBoxMessage;

    public string MessageId => _inBoxMessage.MessageId;
    public string EventName => _inBoxMessage.EventName;
    public string ProcessorId => _inBoxMessage.ProcessorId;
    public int NumberAttempts => _inBoxMessage.NumberAttempts;
    public string Status => _inBoxMessage.Status;
    public long ElapsedMilliseconds => _inBoxMessage.ElapsedMilliseconds;
}