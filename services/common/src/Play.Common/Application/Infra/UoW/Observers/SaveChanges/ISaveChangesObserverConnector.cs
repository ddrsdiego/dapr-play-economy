namespace Play.Common.Application.Infra.UoW.Observers.SaveChanges;

public interface ISaveChangesObserverConnector
{
    IConnectHandle ConnectSaveChangesObserver(ISaveChangesObserver observer);
}