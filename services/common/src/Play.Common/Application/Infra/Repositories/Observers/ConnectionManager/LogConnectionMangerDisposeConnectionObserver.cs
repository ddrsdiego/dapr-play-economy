namespace Play.Common.Application.Infra.Repositories.Observers.ConnectionManager;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UoW.Observers.SaveChanges;

internal sealed class LogConnectionMangerDisposeConnectionObserver : IConnectionMangerDisposeConnectionObserver
{
    private readonly ILogger _logger;

    public LogConnectionMangerDisposeConnectionObserver(ILogger logger) => _logger = logger;

    public Task OnPreCloseConnection(ConnectionManagerLogAudit logAudit) => Task.CompletedTask;

    public Task OnPostCloseConnection(ConnectionManagerLogAudit logAudit)
    {
        const int connectionLifetimeThreshold = 400;

        logAudit.StopWatchConnectionLifetime();

        if (logAudit.ConnectionLifetimeElapsedMilliseconds >= connectionLifetimeThreshold)
        {
            _logger.LogWarning($"[{LogFields.LogType}] - {LogFields.ElapsedMilliseconds}",
                "on-post-close-connection",
                logAudit.ConnectionLifetimeElapsedMilliseconds);
        }

        return Task.CompletedTask;
    }

    public Task OnErrorCloseConnection(ConnectionManagerLogAudit logAudit, Exception e)
    {
        return Task.CompletedTask;
    }
}