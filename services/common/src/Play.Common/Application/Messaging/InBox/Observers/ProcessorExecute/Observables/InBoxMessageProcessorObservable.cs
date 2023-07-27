namespace Play.Common.Application.Messaging.InBox.Observers.ProcessorExecute.Observables;

using System;
using System.Threading.Tasks;
using Play.Common.Application.Messaging.InBox;

public sealed class InBoxMessageProcessorObservable :
    Connectable<IInBoxMessageProcessorObserver>, IInBoxMessageProcessorObserver
{
    public Task OnPreProcess(InBoxMessage inBoxMessage) =>
        ForEachAsync(x => x.OnPreProcess(inBoxMessage));

    public Task OnPostProcess(InBoxMessage inBoxMessage) =>
        ForEachAsync(x => x.OnPostProcess(inBoxMessage));

    public Task OnProcessError(InBoxMessage inBoxMessage, Exception exception) =>
        ForEachAsync(x => x.OnProcessError(inBoxMessage, exception));
}