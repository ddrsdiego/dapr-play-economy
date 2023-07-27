namespace Play.Common.Application.Infra.Repositories;

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

public abstract class PostgresRepository : Repository
{
    protected PostgresRepository(ILogger logger, IConnectionManagerFactory connectionManager)
        : base(logger, connectionManager)
    {
    }

    protected static Task SetDataBaseTimeZoneAsync(IDbConnection connection,
        CancellationToken cancellationToken = default)
    {
        const string sql = "SET TIME ZONE 'America/Sao_Paulo'";
        return connection.ExecuteAsync(sql, cancellationToken);
    }
}