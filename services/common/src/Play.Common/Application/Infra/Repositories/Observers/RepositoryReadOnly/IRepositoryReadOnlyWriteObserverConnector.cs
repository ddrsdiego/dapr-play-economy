namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly;

public interface IRepositoryReadOnlyWriteObserverConnector
{
    IConnectHandle Connect(IRepositoryReadOnlyWriteObserver observer);
}