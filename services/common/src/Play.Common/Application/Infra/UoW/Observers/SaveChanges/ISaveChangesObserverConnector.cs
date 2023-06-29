namespace Play.Common.Application.Infra.UoW.Observers.SaveChanges;

using LogCo.Delivery.GestaoEntregas.RouterAdapter.CrossCutting.Commons;

public interface ISaveChangesObserverConnector
{
    IConnectHandle ConnectSaveChangesObserver(ISaveChangesObserver observer);
}