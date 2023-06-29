namespace Play.Common.Application.Infra.UoW;

using System;
using System.Threading;
using System.Threading.Tasks;
using LogCo.Delivery.GestaoEntregas.RouterAdapter.CrossCutting.Commons;
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
    /// Create a new unit of work.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitOfWorkContextId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IUnitOfWork> CreateAsync(string unitOfWorkContextId, CancellationToken cancellationToken = default);
}

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IConnectionManager _connectionManager;
    private readonly EnqueueWorkObservable _enqueueWorkObservable;
    private readonly SaveChangesObservable _saveChangesObservable;

    public UnitOfWorkFactory(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
        _enqueueWorkObservable = new EnqueueWorkObservable();
        _saveChangesObservable = new SaveChangesObservable();
    }

    public Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default) => CreateAsync(Guid.NewGuid().ToString(), cancellationToken);

    public async Task<IUnitOfWork> CreateAsync(string unitOfWorkContextId, CancellationToken cancellationToken = default)
    {
        var unitOfWork = UnitOfWorkPostgres.Create(unitOfWorkContextId, _connectionManager, cancellationToken);

        unitOfWork.ConnectEnqueueWorkObserver(_enqueueWorkObservable);
        unitOfWork.ConnectSaveChangesObserver(_saveChangesObservable);

        await unitOfWork.BeginTransactionAsync();

        return unitOfWork;
    }

    public IConnectHandle ConnectEnqueueWorkObserver(IEnqueueWorkObserver observer) => _enqueueWorkObservable.Connect(observer);

    public IConnectHandle ConnectSaveChangesObserver(ISaveChangesObserver observer) => _saveChangesObservable.Connect(observer);
}