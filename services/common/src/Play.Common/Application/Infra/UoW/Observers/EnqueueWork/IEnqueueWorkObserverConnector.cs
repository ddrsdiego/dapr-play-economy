namespace Play.Common.Application.Infra.UoW.Observers.EnqueueWork;

using LogCo.Delivery.GestaoEntregas.RouterAdapter.CrossCutting.Commons;

public interface IEnqueueWorkObserverConnector
{
    IConnectHandle ConnectEnqueueWorkObserver(IEnqueueWorkObserver observer);
}