namespace Play.Common.Application.Infra.UoW;

using System.Threading.Tasks;
using Observers.EnqueueWork;
using Observers.EnqueueWork.Observables;
using Observers.SaveChanges;
using Observers.SaveChanges.Observables;
using Repositories;

public interface IUnitOfWorkFactory :
    IEnqueueWorkObserverConnector,
    ISaveChangesObserverConnector
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ValueTask<IUnitOfWork> CreateAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitOfWorkContextId"></param>
    /// <returns></returns>
    ValueTask<IUnitOfWork> CreateAsync(string unitOfWorkContextId);
}

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly EnqueueWorkObservable _enqueueWorkObservable;
    private readonly SaveChangesObservable _saveChangesObservable;
    private readonly ITransactionManagerFactory _transactionFactory;

    public UnitOfWorkFactory(ITransactionManagerFactory transactionManagerFactory)
    {
        _transactionFactory = transactionManagerFactory;
        _enqueueWorkObservable = new EnqueueWorkObservable();
        _saveChangesObservable = new SaveChangesObservable();
    }

    public ValueTask<IUnitOfWork> CreateAsync() => CreateAsync(GeneratorOperationId.Generate());

    public async ValueTask<IUnitOfWork> CreateAsync(string unitOfWorkContextId)
    {
        var unitOfWork = UnitOfWorkPostgres.Create(unitOfWorkContextId, _transactionFactory);

        unitOfWork.ConnectEnqueueWorkObserver(_enqueueWorkObservable);
        unitOfWork.ConnectSaveChangesObserver(_saveChangesObservable);

        await unitOfWork.BeginTransactionAsync();

        return unitOfWork;
    }

    public IConnectHandle ConnectEnqueueWorkObserver(IEnqueueWorkObserver observer) => _enqueueWorkObservable.Connect(observer);

    public IConnectHandle ConnectSaveChangesObserver(ISaveChangesObserver observer) => _saveChangesObservable.Connect(observer);
}