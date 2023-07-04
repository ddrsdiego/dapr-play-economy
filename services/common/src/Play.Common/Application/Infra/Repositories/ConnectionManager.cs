namespace Play.Common.Application.Infra.Repositories;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Polly;

public interface IConnectionManager : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// This method closes the connection asynchronously.
    /// It returns a task that can wait until the connection is closed.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellationToken parameter can be used to cancel the operation.</param>
    Task CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// This method returns an asynchronously opened database connection.
    /// It returns a task that can wait until the connection is ready.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellationToken parameter can be used to cancel the operation</param>
    /// <returns>The return type is DbConnection, which represents a generic database connection.</returns>
    Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// This method returns an asynchronously opened database connection.
    /// It returns a task that can wait until the connection is ready.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellationToken parameter can be used to cancel the operation</param>
    /// <returns>The return type is DbConnection, which represents a generic database connection.</returns>
    Task<DbConnection> GetOpenConnectionWithTransactionAsync(CancellationToken cancellationToken = default);
}

public sealed class ConnectionManager : IConnectionManager
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _semaphore;
    private readonly IAsyncPolicy _resiliencePolicy;
    private readonly DbProviderFactory _providerFactory;
    private readonly ITransactionManagerFactory _transactionManagerFactory;

    private bool _disposed;
    private DbConnection _dbConnection;
    private ITransactionManager _transactionScopeManager;

    public ConnectionManager(DbProviderFactory providerFactory, ITransactionManagerFactory transactionManagerFactory, string connectionString, IAsyncPolicy resiliencePolicy = null)
    {
        _providerFactory = providerFactory;
        _transactionManagerFactory = transactionManagerFactory;
        _connectionString = connectionString;
        _resiliencePolicy = resiliencePolicy;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await CloseAsync(cancellationToken);

            if (_resiliencePolicy != null)
                _dbConnection = await _resiliencePolicy.ExecuteAsync(
                    async () => await TryOpenConnectionAsync(cancellationToken));
            else
                _dbConnection = await TryOpenConnectionAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return _dbConnection;
    }

    public Task<DbConnection> GetOpenConnectionWithTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transactionScopeManager = _transactionManagerFactory.CreateTransactionManager();
        return GetOpenConnectionAsync(cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_dbConnection is { State: ConnectionState.Open })
        {
            _transactionScopeManager?.Dispose();
            await _dbConnection.CloseAsync();
        }
    }

    private async Task<DbConnection> TryOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        DbConnection dbConnection;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            dbConnection = _providerFactory.CreateConnection();
            if (dbConnection == null)
            {
                throw new ArgumentNullException(nameof(dbConnection),
                    "The database connection could not be created. Please check the provider factory and ensure it returns a valid connection object.");
            }

            dbConnection.ConnectionString = _connectionString;

            if (dbConnection.State != ConnectionState.Open)
            {
                await dbConnection.OpenAsync(cancellationToken);
            }
        }
        catch (NpgsqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (DbException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return dbConnection;
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        Dispose();
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
        {
            _dbConnection?.Dispose();
            _transactionScopeManager?.Dispose();
            _dbConnection = null;
        }

        _disposed = true;
    }

    ~ConnectionManager()
    {
        Dispose(false);
    }
}