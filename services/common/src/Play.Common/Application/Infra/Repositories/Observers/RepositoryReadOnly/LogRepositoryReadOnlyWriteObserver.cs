namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

internal sealed class LogRepositoryReadOnlyWriteObserver :
    IRepositoryReadOnlyWriteObserver
{
    private readonly ILogger _logger;

    public LogRepositoryReadOnlyWriteObserver(ILogger logger) => _logger = logger;

    public Task OnPreProcess(WriteRequest request)
    {
        return Task.CompletedTask;
    }

    public Task OnPostProcess(WriteRequest request)
    {
        return Task.CompletedTask;
    }

    public Task OnErrorProcess(WriteRequest request, Exception e)
    {
        return Task.CompletedTask;
    }
}