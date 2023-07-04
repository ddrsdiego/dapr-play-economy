namespace Play.Common.Application.Infra.Repositories;

using System;
using System.Transactions;

public interface ITransactionManager : IDisposable
{
    void Rollback();
}

internal sealed class TransactionManager : ITransactionManager
{
    private bool _disposed;
    private const int MaximumTimeoutInSeconds = 1;
    private TransactionScope _transactionScope;

    public TransactionManager()
    {
        var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };

        _transactionScope = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
        _disposed = false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Rollback()
    {
        _transactionScope?.Dispose();
        _transactionScope = null;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _transactionScope?.Complete();
            _transactionScope?.Dispose();
            _transactionScope = null;
        }

        _disposed = true;
    }

    ~TransactionManager()
    {
        Dispose(false);
    }
}