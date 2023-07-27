namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly;

using Play.Common;

public interface IRepositoryReadOnlyReadObserverConnector
{
    IConnectHandle Connect(IRepositoryReadOnlyReadObserver observer);
}