namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed class ReadRequestAudit
{
    private readonly Stopwatch _stopwatch = new();

    public ReadRequestAudit(ReadRequest readRequest) => ReadRequest = readRequest;

    internal readonly ReadRequest ReadRequest;
    
    internal void Stop() => _stopwatch.Stop();
    internal void Start() => _stopwatch.Start();
    
    internal long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;
}

public interface IRepositoryReadOnlyReadObserver
{
    Task OnPreProcess(ReadRequestAudit readRequestAudit);
    Task OnCacheMiss(ReadRequestAudit readRequestAudit);
    Task OnPostProcess(ReadRequestAudit readRequestAudit);
    Task OnErrorProcess(ReadRequestAudit readRequestAudit, Exception e);
}