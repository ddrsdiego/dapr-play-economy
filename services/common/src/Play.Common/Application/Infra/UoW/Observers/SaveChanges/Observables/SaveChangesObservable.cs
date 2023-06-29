namespace Play.Common.Application.Infra.UoW.Observers.SaveChanges.Observables;

using System;
using System.Threading.Tasks;
using LogCo.Delivery.GestaoEntregas.RouterAdapter.CrossCutting.Commons;

public class SaveChangesObservable :
    Connectable<ISaveChangesObserver>, ISaveChangesObserver
{
    public Task OnPreProcess(UnitOfWorkProcess unitOfWorkProcess)
    {
        return ForEachAsync(x => x.OnPreProcess(unitOfWorkProcess));
    }

    public Task OnPostProcess(UnitOfWorkProcess unitOfWorkProcess)
    {
        return ForEachAsync(x => x.OnPostProcess(unitOfWorkProcess));
    }

    public Task OnErrorProcess(UnitOfWorkProcess unitOfWorkProcess, Exception e)
    {
        return ForEachAsync(x => x.OnErrorProcess(unitOfWorkProcess, e));
    }
}