namespace Play.Common.Application.Infra.Repositories;

using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Polly;

public interface IConnectionManagerFactory
{
    IConnectionManager CreateConnection();
}

public sealed class ConnectionManagerFactory : IConnectionManagerFactory
{
    private const string postgresConnectionStringSettings = "PostgresConnectionString";

    private readonly string _connectionString;
    private readonly IAsyncPolicy _resiliencePolicy;
    private readonly DbProviderFactory _providerFactory;
    private readonly ITransactionManagerFactory _transactionManagerFactory;

    public ConnectionManagerFactory(IConfiguration configuration, DbProviderFactory providerFactory, ITransactionManagerFactory transactionManagerFactory, IAsyncPolicy resiliencePolicy = null)
    {
        _connectionString = configuration.GetConnectionString(postgresConnectionStringSettings);
        _providerFactory = providerFactory;
        _resiliencePolicy = resiliencePolicy;
        _transactionManagerFactory = transactionManagerFactory;
    }

    public IConnectionManager CreateConnection()
    {
        return new ConnectionManager(_providerFactory, _transactionManagerFactory, _connectionString, _resiliencePolicy);
    }
}