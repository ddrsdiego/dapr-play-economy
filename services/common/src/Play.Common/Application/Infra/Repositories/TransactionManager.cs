﻿namespace Play.Common.Application.Infra.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;

public interface ITransactionManager : IDisposable
{
    void BeginTransaction();

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}

internal sealed class TransactionManager : ITransactionManager
{
    private const int MaximumTimeoutInSeconds = 1;

    private TransactionScope _transactionScope;
    private bool _disposed;

    public void BeginTransaction()
    {
        if (_transactionScope != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
        };

        _transactionScope =
            new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transactionScope == null)
            throw new InvalidOperationException("There is no transaction in progress.");

        _transactionScope.Complete();

        TryDisposeTransactionScope();

        return Task.CompletedTask;
    }

    private void TryDisposeTransactionScope()
    {
        _transactionScope.Dispose();
        _transactionScope = null;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transactionScope == null)
            throw new InvalidOperationException("There is no transaction in progress.");

        TryDisposeTransactionScope();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transactionScope?.Dispose();
            }

            _disposed = true;
        }
    }

    ~TransactionManager()
    {
        Dispose(false);
    }
}