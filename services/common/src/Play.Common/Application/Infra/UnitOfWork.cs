namespace Play.Common.Application.Infra;

using System;
using System.Threading;
using System.Threading.Tasks;
using Repositories;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IConnectionManager ConnectionManager { get; }

    Task InitializeTransactionAsync(CancellationToken cancellationToken = default);

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

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public abstract class UnitOfWork : IUnitOfWork
{
    private readonly CancellationToken _cancellationToken;
    private bool _committed;
    private bool _disposed;

    protected UnitOfWork(IConnectionManager connectionManager, CancellationToken cancellationToken = default)
    {
        ConnectionManager = connectionManager;
        _cancellationToken = cancellationToken;
    }

    public IConnectionManager ConnectionManager { get; private set; }

    public virtual async Task InitializeTransactionAsync(CancellationToken cancellationToken = default)
    {
        await ConnectionManager.GetOpenConnectionAsync(cancellationToken);
        await ConnectionManager.BeginTransaction(cancellationToken);
    }

    public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionManager.TransactionManager.CommitAsync(_cancellationToken);
    }

    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_committed) return;
        await ConnectionManager.TransactionManager?.RollbackAsync(_cancellationToken);
    }

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_committed)
            {
                if (ConnectionManager.TransactionManager == null)
                {
                    throw new InvalidOperationException("There is no transaction in progress.");
                }

                await ConnectionManager.TransactionManager.CommitAsync(_cancellationToken);
                ConnectionManager.TransactionManager.Dispose();
            
                _committed = true;
            }
        }
        catch (Exception e)
        {
            await DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ConnectionManager.CloseAsync(_cancellationToken);
        await ConnectionManager.DisposeAsync();

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            ConnectionManager?.TransactionManager?.Dispose();
            ConnectionManager = null;
        }

        _disposed = true;
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}