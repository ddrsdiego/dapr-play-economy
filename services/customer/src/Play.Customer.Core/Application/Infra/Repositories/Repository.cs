namespace Play.Customer.Core.Application.Infra.Repositories
{
    using System.Data;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Npgsql;

    public sealed class ConnectionStringOptions
    {
        public string PostgresConnection { get; set; }
    }
    
    public abstract class Repository
    {
        private readonly IOptions<ConnectionStringOptions> _connectionString;

        protected Repository(ILogger logger, IOptions<ConnectionStringOptions> connectionString)
        {
            Logger = logger;
            _connectionString = connectionString;
        }

        protected ILogger Logger { get; }
        protected string ConnectionString => _connectionString.Value.PostgresConnection;

        protected IDbConnection GetConnection() => new NpgsqlConnection(ConnectionString);
    }
}