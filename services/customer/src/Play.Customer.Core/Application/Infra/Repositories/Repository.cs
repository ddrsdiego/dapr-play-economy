namespace Play.Customer.Core.Application.Infra.Repositories;

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Infra.Repositories;
using Microsoft.Extensions.Logging;

public sealed class ConnectionStringOptions
{
    public string PostgresConnection { get; set; }
}

public abstract class Repository
{
    private readonly IConnectionManager _connectionManager;

    protected Repository(ILogger logger, IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
        Logger = logger;
    }

    protected ILogger Logger { get; }

    protected Task<DbConnection> GetConnection(CancellationToken cancellationToken = default) => _connectionManager.GetOpenConnectionAsync(cancellationToken);
}