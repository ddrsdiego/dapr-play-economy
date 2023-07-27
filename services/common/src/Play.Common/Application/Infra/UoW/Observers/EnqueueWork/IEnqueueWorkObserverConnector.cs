namespace Play.Common.Application.Infra.UoW.Observers.EnqueueWork;

public interface IEnqueueWorkObserverConnector
{
    IConnectHandle ConnectEnqueueWorkObserver(IEnqueueWorkObserver observer);
}