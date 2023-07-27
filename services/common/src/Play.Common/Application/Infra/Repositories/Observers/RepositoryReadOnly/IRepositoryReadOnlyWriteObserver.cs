namespace Play.Common.Application.Infra.Repositories.Observers.RepositoryReadOnly;

using System;
using System.Threading.Tasks;

public interface IRepositoryReadOnlyWriteObserver
{
    Task OnPreProcess(WriteRequest request);
    Task OnPostProcess(WriteRequest request);
    Task OnErrorProcess(WriteRequest request, Exception e);
}