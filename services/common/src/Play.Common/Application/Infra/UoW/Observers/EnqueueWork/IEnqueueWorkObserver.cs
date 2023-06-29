namespace Play.Common.Application.Infra.UoW.Observers.EnqueueWork;

using System.Threading.Tasks;

public interface IEnqueueWorkObserver
{
    Task OnPreProcess();
    Task OnPostProcess();
    Task OnErrorProcess();
}