namespace Play.Common.Application.Messaging.InBox.Observers.ProcessorExecute;

using System;
using System.Threading.Tasks;
using Play.Common.Application.Messaging.InBox;

public interface IInBoxMessageProcessorObserver
{
    Task OnPreProcess(InBoxMessage inBoxMessage);
    Task OnPostProcess(InBoxMessage inBoxMessage);
    Task OnProcessError(InBoxMessage inBoxMessage, Exception exception);
}