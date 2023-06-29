namespace Play.Common.Application.Infra.UoW.Observers.SaveChanges;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class LogFields
{
    public const string LogType = "{log}";
    public const string StepProcess = "{step-process}";
    public const string OutBoxRequest = "{out-box-request}";
    public const string RouterLogContextId = "{router-log-context-id}";
    public const string ElapsedMilliseconds = "{elapsed-ms}";
    public const string OutBoxMessageLogContext = "{@OutBoxMessageLogContext}";
    public const string ItineraryFromRouterLogContext = "{@ItineraryFromRouterLogContext}";
    public const string UnitOfWorkProcessLogContext = "{@UnitOfWorkProcessLogContext}";
}

internal class UnitOfWorkProcessLogContext
{
    private readonly UnitOfWorkProcess _unitOfWorkProcess;

    public UnitOfWorkProcessLogContext(UnitOfWorkProcess unitOfWorkProcess) => _unitOfWorkProcess = unitOfWorkProcess;

    public string UnitOfWorkContextId => _unitOfWorkProcess.UnitOfWorkContextId;
    public string WorkId => _unitOfWorkProcess.WorkId;
    public long ElapsedMilliseconds => _unitOfWorkProcess.ElapsedMilliseconds;
}

public sealed class LogSaveChangesObserver :
    ISaveChangesObserver
{
    private readonly ILogger _logger;
    private const string SaveChangesLogMessage = "unit-of-work-save-changes";

    public LogSaveChangesObserver(IServiceProvider serviceProvider) =>
        _logger = serviceProvider.GetRequiredService<ILogger<LogSaveChangesObserver>>();

    public Task OnPreProcess(UnitOfWorkProcess unitOfWorkProcess)
    {
        var logContext = new UnitOfWorkProcessLogContext(unitOfWorkProcess);

        _logger.LogInformation($"[{LogFields.LogType}] - {LogFields.StepProcess} - {LogFields.UnitOfWorkProcessLogContext}",
            SaveChangesLogMessage,
            "OnPreProcess",
            logContext);

        return Task.CompletedTask;
    }

    public Task OnPostProcess(UnitOfWorkProcess unitOfWorkProcess)
    {
        unitOfWorkProcess.StopWatch();
        var logContext = new UnitOfWorkProcessLogContext(unitOfWorkProcess);

        _logger.LogInformation($"[{LogFields.LogType}] - {LogFields.StepProcess} - {LogFields.UnitOfWorkProcessLogContext}",
            SaveChangesLogMessage,
            "OnPostProcess",
            logContext);

        return Task.CompletedTask;
    }

    public Task OnErrorProcess(UnitOfWorkProcess unitOfWorkProcess, Exception e)
    {
        unitOfWorkProcess.StopWatch();
        var logContext = new UnitOfWorkProcessLogContext(unitOfWorkProcess);

        _logger.LogError(e, $"[{LogFields.LogType}] - {LogFields.StepProcess} - {LogFields.UnitOfWorkProcessLogContext}",
            SaveChangesLogMessage,
            "OnErrorProcess",
            logContext);

        return Task.CompletedTask;
    }
}