namespace Play.Common.Application.Infra.UoW;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Repositories;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IConnectionManager ConnectionManager { get; }

    Task BeginTransactionAsync();

    Task SaveChangesAsync();

    void AddToContext(Func<Task> task);
}

public abstract class UnitOfWork : IUnitOfWork
{
    private bool _committed;
    private bool _disposed;
    private LinkedList<UnitOfWorkProcess> _methods;

    private readonly object _syncLock = new();
    private readonly CancellationToken _cancellationToken;

    protected UnitOfWork(string unitOfWorkContextId, IConnectionManager connectionManager, CancellationToken cancellationToken = default)
    {
        _methods = new LinkedList<UnitOfWorkProcess>();

        UnitOfWorkContextId = unitOfWorkContextId;
        ConnectionManager = connectionManager;
        _cancellationToken = cancellationToken;
    }

    public string UnitOfWorkContextId { get; }

    public IConnectionManager ConnectionManager { get; }

    public virtual async Task BeginTransactionAsync()
    {
        try
        {
            _cancellationToken.ThrowIfCancellationRequested();

            await ConnectionManager.BeginTransactionAsync(_cancellationToken);
            _committed = false;
        }
        catch (OperationCanceledException e)
        {
            await DisposeAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public virtual async Task SaveChangesAsync()
    {
        try
        {
            if (_methods.Count == 0) return;

            _cancellationToken.ThrowIfCancellationRequested();

            LinkedList<UnitOfWorkProcess> localMethods;

            lock (_syncLock)
            {
                localMethods = _methods;
                _methods = new LinkedList<UnitOfWorkProcess>();
            }

            foreach (var process in localMethods)
            {
                await process.Method().ConfigureAwait(false);
            }

            if (!_committed)
            {
                if (ConnectionManager.TransactionManager == null)
                {
                    throw new InvalidOperationException("There is no transaction in progress.");
                }

                await ConnectionManager.TransactionManager.CommitAsync(_cancellationToken);
                _committed = true;
            }
        }
        catch (OperationCanceledException e)
        {
            await DisposeAsync();
        }
        catch (InvalidOperationException e)
        {
            await DisposeAsync();
            throw;
        }
    }

    public void AddToContext(Func<Task> task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        lock (_syncLock)
        {
            _methods.AddLast(new UnitOfWorkProcess(UnitOfWorkContextId, task));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (ConnectionManager?.TransactionManager != null)
        {
            ConnectionManager.TransactionManager.Dispose();
            await ConnectionManager.CloseAsync(_cancellationToken);
        }

        Dispose();
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
            lock (_syncLock)
            {
                _methods = null;
            }

            ConnectionManager?.TransactionManager?.Dispose();
        }

        _disposed = true;
    }

    ~UnitOfWork() => Dispose(false);
}