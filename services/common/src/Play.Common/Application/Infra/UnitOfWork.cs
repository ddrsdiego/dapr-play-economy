namespace Play.Common.Application.Infra;

using System;
using System.Threading;
using System.Threading.Tasks;
using Repositories;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IConnectionManager ConnectionManager { get; }

    /// <summary>
    /// Start a new transaction.
    /// </summary>
    /// <returns></returns>
    void BeginTransaction();

    /// <summary>
    /// End the current transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Undo the current transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsyncAsync(CancellationToken cancellationToken = default);
}

public abstract class UnitOfWork : IUnitOfWork
{
    private readonly CancellationToken _cancellationToken;

    protected UnitOfWork(IConnectionManager connectionManager, CancellationToken cancellationToken = default)
    {
        ConnectionManager = connectionManager;
        _cancellationToken = cancellationToken;
    }

    public IConnectionManager ConnectionManager { get; }

    public abstract Task OpenAsync(CancellationToken cancellationToken = default);

    public abstract Task CloseAsync(CancellationToken cancellationToken = default);

    public virtual void BeginTransaction() => ConnectionManager.TransactionManager.BeginTransaction();

    public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionManager.TransactionManager.CommitAsync(_cancellationToken);
    }

    public virtual Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionManager.TransactionManager.RollbackAsync(_cancellationToken);
    }

    public abstract Task SaveChangesAsyncAsync(CancellationToken cancellationToken = default);

    public async ValueTask DisposeAsync()
    {
        await ConnectionManager.TransactionManager.CommitAsync(_cancellationToken);
        ConnectionManager.TransactionManager.Dispose();

        await ConnectionManager.CloseAsync(_cancellationToken);
        await ConnectionManager.DisposeAsync();

        Dispose();
    }

    public void Dispose() => GC.SuppressFinalize(this);
}