namespace Play.Common.Application.Infra.UoW;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Repositories;

public interface IUnitOfWork :
    IDisposable,
    IAsyncDisposable
{
    IConnectionManager ConnectionManager { get; }

    Task BeginTransactionAsync();

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    int QueueLength { get; }

    void AddToContext(Func<Task> method);
}

public abstract class UnitOfWork : IUnitOfWork
{
    private bool _committed;
    private bool _disposed;
    private bool _newTransaction;

    private readonly SemaphoreSlim _commitLock;
    private readonly CancellationToken _cancellationToken;
    private ConcurrentQueue<UnitOfWorkProcess> _methods;

    protected UnitOfWork(string unitOfWorkContextId, IConnectionManager connectionManager, CancellationToken cancellationToken = default)
    {
        ConnectionManager = connectionManager;
        UnitOfWorkContextId = unitOfWorkContextId;

        _cancellationToken = cancellationToken;
        _methods = new ConcurrentQueue<UnitOfWorkProcess>();
        _commitLock = new SemaphoreSlim(1, 1);
    }

    public string UnitOfWorkContextId { get; }

    public IConnectionManager ConnectionManager { get; }

    public int QueueLength => _methods.Count;

    public virtual async Task BeginTransactionAsync()
    {
        try
        {
            _cancellationToken.ThrowIfCancellationRequested();

            await ConnectionManager.BeginTransactionAsync(_cancellationToken);

            _committed = false;
            _newTransaction = true;
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

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_methods.IsEmpty) return;

            cancellationToken.ThrowIfCancellationRequested();

            await _commitLock.WaitAsync(cancellationToken);

            try
            {
                if (_newTransaction)
                {
                    while (_methods.TryDequeue(out var process))
                    {
                        await process.Method().ConfigureAwait(false);
                    }

                    await CommitAsync();
                    _newTransaction = false;
                }
                else
                    throw new InvalidOperationException("There is no transaction in progress.");
            }
            finally
            {
                _commitLock.Release();
            }
        }
        catch (OperationCanceledException e)
        {
            await DisposeAsync();
        }
        catch (InvalidOperationException e)
        {
            await DisposeAsync();
        }
    }

    private async Task CommitAsync()
    {
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

    public void AddToContext(Func<Task> method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        _methods.Enqueue(new UnitOfWorkProcess(UnitOfWorkContextId, method));
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        ConnectionManager?.TransactionManager?.Dispose();
        await ConnectionManager?.CloseAsync(_cancellationToken)!;

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
        }

        _disposed = true;
    }

    ~UnitOfWork() => Dispose(false);
}