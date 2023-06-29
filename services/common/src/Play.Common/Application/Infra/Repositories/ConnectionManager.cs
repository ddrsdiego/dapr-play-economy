namespace Play.Common.Application.Infra.Repositories;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Polly;

public interface IConnectionManager : IDisposable
{
    string ConnectionString { get; }

    /// <summary>
    /// This method starts an asynchronous transaction.
    /// It returns a task that can wait until the transaction starts.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellationToken parameter can be used to cancel the operation</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    ITransactionManager TransactionManager { get; }

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
}

public sealed class ConnectionManager : IConnectionManager
{
    private readonly SemaphoreSlim _semaphore;
    private readonly IAsyncPolicy _resiliencePolicy;
    private readonly DbProviderFactory _providerFactory;

    public ConnectionManager(DbProviderFactory providerFactory, string connectionString,
        IAsyncPolicy resiliencePolicy = null)
    {
        _providerFactory = providerFactory;
        ConnectionString = connectionString;
        _resiliencePolicy = resiliencePolicy;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            TransactionManager ??= new TransactionManager();
            TransactionManager.BeginTransaction();

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public ITransactionManager TransactionManager { get; private set; }

    public string ConnectionString { get; set; }

    public async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        DbConnection connection;

        try
        {
            if (_resiliencePolicy != null)
                connection = await _resiliencePolicy.ExecuteAsync(
                    async () => await TryOpenConnectionAsync(cancellationToken));
            else
                connection = await TryOpenConnectionAsync(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return connection;
    }

    public Task CloseAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

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
            
            dbConnection.ConnectionString = ConnectionString;

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

    public void Dispose()
    {
        TransactionManager?.Dispose();
        TransactionManager = null;
    }
}