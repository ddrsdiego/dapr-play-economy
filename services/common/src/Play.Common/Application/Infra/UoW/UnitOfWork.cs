namespace Play.Common.Application.Infra.UoW;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Observers.EnqueueWork.Observables;
using Observers.SaveChanges.Observables;
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

    Task AddToContextAsync(Func<Task> method);

    Task AddToContextAsync(string workId, Func<Task> method);

    void ConnectEnqueueWorkObserver(EnqueueWorkObservable enqueueWorkObservable);

    void ConnectSaveChangesObserver(SaveChangesObservable saveChangesObservable);
}

public abstract class UnitOfWork : IUnitOfWork
{
    private bool _committed;
    private bool _disposed;
    private bool _newTransaction;

    private readonly SemaphoreSlim _commitLock;
    private readonly CancellationToken _cancellationToken;
    private EnqueueWorkObservable _enqueueWorkObservable;
    private SaveChangesObservable _saveChangesObservable;

    private ConcurrentQueue<UnitOfWorkProcess> _methods;

    protected UnitOfWork(string unitOfWorkContextId, IConnectionManager connectionManager, CancellationToken cancellationToken = default)
    {
        ConnectionManager = connectionManager;
        UnitOfWorkContextId = $"{unitOfWorkContextId}-{GeneratorOperationId.Generate()}";

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
                if (!_newTransaction)
                    throw new InvalidOperationException("There is no transaction in progress.");

                while (_methods.TryDequeue(out var process))
                {
                    try
                    {
                        await _saveChangesObservable.OnPreProcess(process);

                        await process.Method().ConfigureAwait(false);

                        await _saveChangesObservable.OnPostProcess(process);
                    }
                    catch (Exception e)
                    {
                        await _saveChangesObservable.OnErrorProcess(process, e);
                        throw;
                    }
                }

                await CommitAsync();
                _newTransaction = false;
            }
            finally
            {
                _commitLock.Release();
            }
        }
        catch (OperationCanceledException e)
        {
            await DisposeAsync();
            throw;
        }
        catch (InvalidOperationException e)
        {
            await DisposeAsync();
            throw;
        }
        catch (Exception e)
        {
            await DisposeAsync();
            throw;
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

        _methods.Enqueue(new UnitOfWorkProcess(UnitOfWorkContextId, GeneratorOperationId.Generate(), method));
    }

    public Task AddToContextAsync(Func<Task> method) => AddToContextAsync(GeneratorOperationId.Generate(), method);

    public async Task AddToContextAsync(string workId, Func<Task> method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        await _enqueueWorkObservable.OnPreProcess();

        _methods.Enqueue(new UnitOfWorkProcess(UnitOfWorkContextId, workId, method));

        await _enqueueWorkObservable.OnPostProcess();
    }

    public void ConnectEnqueueWorkObserver(EnqueueWorkObservable enqueueWorkObservable) => _enqueueWorkObservable = enqueueWorkObservable;

    public void ConnectSaveChangesObserver(SaveChangesObservable saveChangesObservable) => _saveChangesObservable = saveChangesObservable;

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