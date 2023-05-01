namespace Play.Common.Application.Infra.Repositories;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Polly;

public interface IConnectionManager : IAsyncDisposable
{
    string ConnectionString { get; }

    Task BeginTransaction(CancellationToken cancellationToken = default);

    ITransactionManager TransactionManager { get; }

    Task CloseAsync(CancellationToken cancellationToken = default);

    Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
}

public sealed class ConnectionManager : IConnectionManager
{
    private readonly IAsyncPolicy _resiliencePolicy;
    private readonly DbProviderFactory _providerFactory;

    public ConnectionManager(DbProviderFactory providerFactory, string connectionString,
        IAsyncPolicy resiliencePolicy = null)
    {
        _providerFactory = providerFactory;
        ConnectionString = connectionString;
        _resiliencePolicy = resiliencePolicy;
    }

    private DbConnection _connection;
    private ITransactionManager _transactionManager;

    public Task BeginTransaction(CancellationToken cancellationToken = default)
    {
        TransactionManager ??= new TransactionManager();
        TransactionManager.BeginTransaction();
        
        return Task.CompletedTask;
    }

    public ITransactionManager TransactionManager { get; private set;}

    public string ConnectionString { get; set; }

    public async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_resiliencePolicy != null)
            _connection = await _resiliencePolicy.ExecuteAsync(
                async () => await TryOpenConnectionAsync(cancellationToken));
        else
            _connection = await TryOpenConnectionAsync(cancellationToken);

        return _connection;
    }

    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        return _connection?.State != ConnectionState.Closed ? _connection?.CloseAsync() : Task.CompletedTask;
    }

    private async Task<DbConnection> TryOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        DbConnection dbConnection;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            dbConnection = _providerFactory.CreateConnection();
            dbConnection.ConnectionString = ConnectionString;

            if (dbConnection.State != ConnectionState.Open)
                await dbConnection.OpenAsync(cancellationToken);
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

    public ValueTask DisposeAsync()
    {
        _transactionManager?.Dispose();
        return ValueTask.CompletedTask;
    }
}