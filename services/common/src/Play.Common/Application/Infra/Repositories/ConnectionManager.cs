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

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    ITransactionManager TransactionManager { get; }

    Task CloseAsync(CancellationToken cancellationToken = default);

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
        TransactionManager ??= new TransactionManager();
        TransactionManager.BeginTransaction();

        return Task.CompletedTask;
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