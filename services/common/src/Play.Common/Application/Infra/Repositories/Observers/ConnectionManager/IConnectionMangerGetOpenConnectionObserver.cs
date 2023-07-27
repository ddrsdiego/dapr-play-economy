namespace Play.Common.Application.Infra.Repositories.Observers.ConnectionManager;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed class ConnectionManagerLogAudit
{
    private readonly string _connectionString;
    private readonly Stopwatch _stopwatchOpenConnection = new();
    private readonly Stopwatch _stopwatchConnectionLifetime = new();

    internal ConnectionManagerLogAudit(string connectionString) => _connectionString = connectionString;

    internal void StartWatches()
    {
        _stopwatchOpenConnection.Restart();
        _stopwatchConnectionLifetime.Restart();
    }
    
    internal void StopWatchOpenConnection() => _stopwatchOpenConnection.Stop();
    internal void StopWatchConnectionLifetime() => _stopwatchConnectionLifetime.Stop();
    internal long OpenConnectionElapsedMilliseconds => _stopwatchOpenConnection.ElapsedMilliseconds;
    internal long ConnectionLifetimeElapsedMilliseconds => _stopwatchConnectionLifetime.ElapsedMilliseconds;
}

public interface IConnectionMangerGetOpenConnectionObserver
{
    Task OnPreOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit);
    Task OnPostOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit);
    Task OnErrorOpenConnection(ConnectionManagerLogAudit connectionManagerLogAudit, Exception e);
}