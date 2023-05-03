namespace Play.Common.Application.Infra;

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

public readonly struct UnitOfWorkProcess
{
    public UnitOfWorkProcess(Func<Task> task)
    {
        Method = task;
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; }
    
    public Func<Task> Method { get; }
}

public abstract class UnitOfWork : IUnitOfWork
{
    private bool _committed;
    private bool _disposed;
    private readonly object _syncLock = new();
    private LinkedList<UnitOfWorkProcess> _methods;
    private readonly CancellationToken _cancellationToken;

    protected UnitOfWork(IConnectionManager connectionManager, CancellationToken cancellationToken = default)
    {
        _methods = new LinkedList<UnitOfWorkProcess>();
        ConnectionManager = connectionManager;
        _cancellationToken = cancellationToken;
    }

    public IConnectionManager ConnectionManager { get; private set; }

    public virtual async Task BeginTransactionAsync()
    {
        try
        {
            _cancellationToken.ThrowIfCancellationRequested();

            await ConnectionManager.GetOpenConnectionAsync(_cancellationToken);
            await ConnectionManager.BeginTransactionAsync(_cancellationToken);
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
            _cancellationToken.ThrowIfCancellationRequested();

            foreach (var process in _methods)
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
        catch (Exception e)
        {
            await DisposeAsync();
        }
    }

    public void AddToContext(Func<Task> task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        lock (_syncLock)
        {
            _methods.AddLast(new UnitOfWorkProcess(task));
        }
    }

    public async ValueTask DisposeAsync()
    {
        ConnectionManager.TransactionManager.Dispose();
        await ConnectionManager.CloseAsync(_cancellationToken);
        ConnectionManager.Dispose();

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
            _methods = null;
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