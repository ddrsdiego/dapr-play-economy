namespace Play.Common.Application.Infra.UoW;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Play.Common;
using Play.Common.Application.Infra.Repositories;
using Play.Common.Application.Infra.UoW.Observers.EnqueueWork.Observables;
using Play.Common.Application.Infra.UoW.Observers.SaveChanges.Observables;

public interface IUnitOfWork :
    IDisposable,
    IAsyncDisposable
{
    ValueTask BeginTransactionAsync();

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    int QueueLength { get; }

    void AddToContext(Func<Task> method);

    ValueTask AddToContextAsync(Func<Task> method);

    ValueTask AddToContextAsync(string workId, Func<Task> method);

    void ConnectEnqueueWorkObserver(EnqueueWorkObservable enqueueWorkObservable);

    void ConnectSaveChangesObserver(SaveChangesObservable saveChangesObservable);
}

public abstract class UnitOfWork : IUnitOfWork
{
    private bool _disposed;
    private bool _newTransaction;

    private readonly SemaphoreSlim _commitLock;
    private readonly ITransactionManagerFactory _transactionFactory;

    private EnqueueWorkObservable _enqueueWorkObservable;
    private SaveChangesObservable _saveChangesObservable;

    private ConcurrentQueue<UnitOfWorkProcess> _methods;

    protected UnitOfWork(string unitOfWorkContextId, ITransactionManagerFactory transactionFactory)
    {
        UnitOfWorkContextId = unitOfWorkContextId;

        _transactionFactory = transactionFactory;
        _methods = new ConcurrentQueue<UnitOfWorkProcess>();
        _commitLock = new SemaphoreSlim(1, 1);
    }

    private string UnitOfWorkContextId { get; }

    public int QueueLength => _methods.Count;

    public virtual async ValueTask BeginTransactionAsync()
    {
        try
        {
            _newTransaction = true;
        }
        catch (OperationCanceledException)
        {
            await DisposeAsync();
        }
        catch (Exception)
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

                using var transactionManager = _transactionFactory.CreateTransactionManager();
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
                        transactionManager.Rollback();

                        await _saveChangesObservable.OnErrorProcess(process, e);
                        throw;
                    }
                }
            }
            finally
            {
                _commitLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            await DisposeAsync();
            throw;
        }
        catch (InvalidOperationException)
        {
            await DisposeAsync();
            throw;
        }
        catch (Exception)
        {
            await DisposeAsync();
            throw;
        }
    }

    public void AddToContext(Func<Task> method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        _methods.Enqueue(new UnitOfWorkProcess(UnitOfWorkContextId, GeneratorOperationId.Generate(), method));
    }

    public ValueTask AddToContextAsync(Func<Task> method) => AddToContextAsync(GeneratorOperationId.Generate(), method);

    public async ValueTask AddToContextAsync(string workId, Func<Task> method)
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

        Dispose();
        await Task.CompletedTask;
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
        }

        _disposed = true;
    }

    ~UnitOfWork() => Dispose(false);
}