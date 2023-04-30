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
    // DbConnection Connection { get; }

    ITransactionManager TransactionManager { get; }

    Task CloseAsync(CancellationToken cancellationToken = default);

    Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
}

public sealed class ConnectionManager : IConnectionManager
{
    private readonly DbProviderFactory _providerFactory;
    private readonly string _connectionString;
    private readonly IAsyncPolicy _resiliencePolicy;

    public ConnectionManager(DbProviderFactory providerFactory, string connectionString,
        IAsyncPolicy resiliencePolicy = null)
    {
        _providerFactory = providerFactory;
        _connectionString = connectionString;
        _resiliencePolicy = resiliencePolicy;
        TransactionManager = new TransactionManager();
    }

    // public DbConnection Connection { get; private set; }

    public ITransactionManager TransactionManager { get; }

    public async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        DbConnection connection;

        if (_resiliencePolicy != null)
            connection = await _resiliencePolicy.ExecuteAsync(
                async () => await TryOpenConnectionAsync(cancellationToken));
        else
            connection = await TryOpenConnectionAsync(cancellationToken);

        return connection;
    }

    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        // return Connection.State != ConnectionState.Closed ? Connection.CloseAsync() : Task.CompletedTask;
        return Task.CompletedTask;
    }

    private async Task<DbConnection> TryOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        DbConnection dbConnection;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            dbConnection = _providerFactory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;

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
        TransactionManager?.Dispose();
        return ValueTask.CompletedTask;
    }
}