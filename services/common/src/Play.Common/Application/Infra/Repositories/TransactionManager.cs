namespace Play.Common.Application.Infra.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;

public interface ITransactionManager : IDisposable
{
    void BeginTransaction();

    Task CommitAsync(CancellationToken cancellationToken = default);
}

internal sealed class TransactionManager : ITransactionManager
{
    private const int MaximumTimeoutInSeconds = 1;
    private bool _disposed;
    private TransactionScope _transactionScope;

    public TransactionManager()
    {
        _transactionScope = null;
        _disposed = false;
    }

    public void BeginTransaction()
    {
        if (_transactionScope != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transactionScope = CreateNewTransaction();
    }

    private static TransactionScope CreateNewTransaction()
    {
        return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
        }, TransactionScopeAsyncFlowOption.Enabled);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transactionScope == null)
        {
            throw new InvalidOperationException("There is no transaction in progress.");
        }

        _transactionScope.Complete();
        return Task.CompletedTask;
    }

    private void TryDisposeTransactionScope()
    {
        _transactionScope.Dispose();
        _transactionScope = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            TryDisposeTransactionScope();

        _disposed = true;
    }

    ~TransactionManager()
    {
        Dispose(false);
    }
}