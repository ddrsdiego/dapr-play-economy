namespace Play.Common.Application.Infra.Repositories;

using System;
using System.Transactions;

public interface ITransactionManager : IDisposable
{
    bool IsDisposed { get; }

    bool IsCommitted { get; }

    void Rollback();
}

internal sealed class TransactionManager : ITransactionManager
{
    private readonly object _lockCommit = new();

    private TransactionScope _transactionScope;

    internal TransactionManager() => InitTransaction();

    private void InitTransaction()
    {
        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
        };

        _transactionScope = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
        IsDisposed = false;
        IsCommitted = false;
    }

    public bool IsDisposed { get; private set; }

    public bool IsCommitted { get; private set; }

    public void Dispose()
    {
        lock (_lockCommit)
        {
            Dispose(true);
        }

        GC.SuppressFinalize(this);
    }

    public void Rollback() => DisposeScope();

    private void DisposeScope()
    {
        if (_transactionScope == null)
            return;

        try
        {
            _transactionScope?.Dispose();
        }
        catch (Exception e)
        {
            throw new TransactionManagementException("Error on disposing transaction scope", e);
        }
        finally
        {
            _transactionScope = null;
        }
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            try
            {
                _transactionScope?.Complete();
                IsCommitted = true;
            }
            catch (Exception e) when (e is TransactionAbortedException)
            {
                throw new TransactionManagementException("Error on completing or disposing transaction scope", e);
            }
            finally
            {
                DisposeScope();
            }
        }

        IsDisposed = true;
    }

    ~TransactionManager()
    {
        Dispose(false);
    }
}