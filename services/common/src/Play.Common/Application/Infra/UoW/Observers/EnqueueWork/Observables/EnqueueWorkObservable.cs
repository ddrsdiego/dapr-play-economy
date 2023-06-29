﻿namespace Play.Common.Application.Infra.UoW.Observers.EnqueueWork.Observables;

using System.Threading.Tasks;
using LogCo.Delivery.GestaoEntregas.RouterAdapter.CrossCutting.Commons;

public class EnqueueWorkObservable :
    Connectable<IEnqueueWorkObserver>, IEnqueueWorkObserver
{
    public Task OnPreProcess()
    {
        return ForEachAsync(x => x.OnPreProcess());
    }

    public Task OnPostProcess()
    {
        return ForEachAsync(x => x.OnPostProcess());
    }

    public Task OnErrorProcess()
    {
        return ForEachAsync(x => x.OnErrorProcess());
    }
}