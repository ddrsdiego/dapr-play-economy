namespace Play.Common.Application.Infra.Repositories;

using Microsoft.Extensions.Logging;

public sealed class ConnectionStringOptions
{
    public string PostgresConnection { get; set; }
}
    
public abstract class Repository
{
    protected readonly IConnectionManagerFactory ConnectionManagerFactory;

    protected Repository(ILogger logger, IConnectionManagerFactory connectionManager)
    {
        ConnectionManagerFactory = connectionManager;
        Logger = logger;
    }

    protected ILogger Logger { get; }
}