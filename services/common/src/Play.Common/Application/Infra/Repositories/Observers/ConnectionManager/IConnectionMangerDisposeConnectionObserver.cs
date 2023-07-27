namespace Play.Common.Application.Infra.Repositories.Observers.ConnectionManager;

using System;
using System.Threading.Tasks;

public interface IConnectionMangerDisposeConnectionObserver
{
    Task OnPreCloseConnection(ConnectionManagerLogAudit logAudit);
    Task OnPostCloseConnection(ConnectionManagerLogAudit logAudit);
    Task OnErrorCloseConnection(ConnectionManagerLogAudit logAudit, Exception e);
}

public interface IConnectionMangerDisposeConnectionObserverConnector
{
    IConnectHandle Connect(IConnectionMangerDisposeConnectionObserver observer);
}