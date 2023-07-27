namespace Play.Common.Application.Infra.Repositories.Observers.ConnectionManager;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UoW.Observers.SaveChanges;

internal sealed class LogConnectionMangerGetOpenConnectionObserver : IConnectionMangerGetOpenConnectionObserver
{
    private readonly ILogger _logger;

    public LogConnectionMangerGetOpenConnectionObserver(ILogger logger) => _logger = logger;

    public Task OnPreOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit)
    {
        connectionManagerLogAudit.StartWatches();
        return Task.CompletedTask;
    }

    public Task OnPostOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit)
    {
        connectionManagerLogAudit.StopWatchOpenConnection();

        if (connectionManagerLogAudit.OpenConnectionElapsedMilliseconds <= 50)
            return Task.CompletedTask;
        
        _logger.LogInformation($"[{LogFields.LogType}] - {LogFields.ElapsedMilliseconds}",
            "on-post-open-connection",
            connectionManagerLogAudit.OpenConnectionElapsedMilliseconds);

        return Task.CompletedTask;
    }

    public Task OnErrorOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit, Exception e)
    {
        _logger.LogError(e, "Failed to open connection");
        return Task.CompletedTask;
    }
}