namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly.Observables;

using System;
using System.Threading.Tasks;

public sealed class RepositoryReadOnlyWriteObservable :
    Connectable<IRepositoryReadOnlyWriteObserver>, IRepositoryReadOnlyWriteObserver
{
    public Task OnPreProcess(WriteRequest request)
    {
        return ForEachAsync(x => x.OnPreProcess(request));
    }

    public Task OnPostProcess(WriteRequest request)
    {
        return ForEachAsync(x => x.OnPostProcess(request));
    }

    public Task OnErrorProcess(WriteRequest request, Exception e)
    {
        return ForEachAsync(x => x.OnErrorProcess(request, e));
    }
}