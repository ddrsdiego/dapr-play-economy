namespace Play.Common.Application.Infra.Repositories.Observers.ConnectionManager;

public interface IConnectionMangerGetOpenConnectionObserverConnector
{
    IConnectHandle Connect(IConnectionMangerGetOpenConnectionObserver observer);
}