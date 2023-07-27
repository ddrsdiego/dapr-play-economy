namespace Play.Common.Application.Messaging.InBox.Observers.ProcessorExecute;

public interface IInBoxMessageProcessorObserverConnector
{
    IConnectHandle ConnectInBoxMessageProcessorExecuteObserver(IInBoxMessageProcessorObserver observer);
}