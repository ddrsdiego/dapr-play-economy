namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly.Observables;

using System;
using System.Threading.Tasks;

public class RepositoryReadOnlyReadObservable :
    Connectable<IRepositoryReadOnlyReadObserver>, IRepositoryReadOnlyReadObserver
{
    public Task OnPreProcess(ReadRequestAudit readRequestAudit)
    {
        return ForEachAsync(x => x.OnPreProcess(readRequestAudit));
    }

    public Task OnCacheMiss(ReadRequestAudit readRequestAudit)
    {
        return ForEachAsync(x => x.OnCacheMiss(readRequestAudit));
    }

    public Task OnPostProcess(ReadRequestAudit readRequestAudit)
    {
        return ForEachAsync(x => x.OnPostProcess(readRequestAudit));
    }

    public Task OnErrorProcess(ReadRequestAudit readRequestAudit, Exception e)
    {
        return ForEachAsync(x => x.OnErrorProcess(readRequestAudit, e));
    }
}