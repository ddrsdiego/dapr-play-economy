namespace Play.Common.Application.Infra.UoW.Observers.SaveChanges;

using System;
using System.Threading.Tasks;

public interface ISaveChangesObserver
{
    Task OnPreProcess(UnitOfWorkProcess unitOfWorkProcess);
    Task OnPostProcess(UnitOfWorkProcess unitOfWorkProcess);
    Task OnErrorProcess(UnitOfWorkProcess unitOfWorkProcess, Exception e);
}