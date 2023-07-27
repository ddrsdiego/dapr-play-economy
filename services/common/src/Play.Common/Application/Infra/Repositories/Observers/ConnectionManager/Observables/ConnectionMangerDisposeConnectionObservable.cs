namespace Play.Common.Application.Infra.Repositories.Observers.ConnectionManager.Observables;

using System;
using System.Threading.Tasks;

public sealed class ConnectionMangerDisposeConnectionObservable :
    Connectable<IConnectionMangerDisposeConnectionObserver>, IConnectionMangerDisposeConnectionObserver
{
    public Task OnPreCloseConnection(ConnectionManagerLogAudit logAudit)
    {
        return ForEachAsync(x => x.OnPreCloseConnection(logAudit));
    }

    public Task OnPostCloseConnection(ConnectionManagerLogAudit logAudit)
    {
        return ForEachAsync(x => x.OnPostCloseConnection(logAudit));
    }

    public Task OnErrorCloseConnection(ConnectionManagerLogAudit logAudit, Exception e)
    {
        return ForEachAsync(x => x.OnErrorCloseConnection(logAudit, e));
    }
}