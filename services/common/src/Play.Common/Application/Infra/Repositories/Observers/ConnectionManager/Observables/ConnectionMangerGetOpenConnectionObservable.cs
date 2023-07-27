namespace Play.Common.Application.Infra.Repositories.Observers.ConnectionManager.Observables;

using System;
using System.Threading.Tasks;

internal sealed class ConnectionMangerGetOpenConnectionObservable :
    Connectable<IConnectionMangerGetOpenConnectionObserver>, IConnectionMangerGetOpenConnectionObserver
{
    public Task OnPreOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit)
    {
        return ForEachAsync(x => x.OnPreOpenConnection(connectionManagerLogAudit));
    }

    public Task OnPostOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit)
    {
        return ForEachAsync(x => x.OnPostOpenConnection(connectionManagerLogAudit));
    }

    public Task OnErrorOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit, Exception e)
    {
        return ForEachAsync(x => x.OnErrorOpenConnection(connectionManagerLogAudit, e));
    }
}