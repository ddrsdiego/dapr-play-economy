namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UoW.Observers.SaveChanges;

internal sealed class LogRepositoryReadOnlyReadObserver :
    IRepositoryReadOnlyReadObserver
{
    private readonly ILogger _logger;

    public LogRepositoryReadOnlyReadObserver(ILogger logger) => _logger = logger;

    public Task OnPreProcess(ReadRequestAudit readRequestAudit)
    {
        readRequestAudit.Start();
        return Task.CompletedTask;
    }

    public Task OnCacheMiss(ReadRequestAudit readRequestAudit)
    {
        readRequestAudit.Stop();
        _logger.LogWarning($"[{LogFields.LogType}]");
        return Task.CompletedTask;
    }

    public Task OnPostProcess(ReadRequestAudit readRequestAudit)
    {
        readRequestAudit.Stop();

        _logger.LogInformation($"[{LogFields.LogType}] - {LogFields.Key} / {LogFields.SubKey} / {LogFields.ElapsedMilliseconds}",
            "read-cache-entry-finished",
            readRequestAudit.ReadRequest.Key,
            readRequestAudit.ReadRequest.SubKey,
            readRequestAudit.ElapsedMilliseconds);
        
        return Task.CompletedTask;
    }

    public Task OnErrorProcess(ReadRequestAudit readRequestAudit, Exception e)
    {
        readRequestAudit.Stop();
        throw new NotImplementedException();
    }
}