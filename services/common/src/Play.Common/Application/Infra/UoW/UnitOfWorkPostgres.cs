namespace Play.Common.Application.Infra.UoW;

using System;
using System.Data.Common;
using System.Threading;
using Npgsql;
using Repositories;
using Polly;

public sealed class UnitOfWorkPostgres : UnitOfWork
{
    private static readonly IAsyncPolicy ResiliencePolicy;

    static UnitOfWorkPostgres()
    {
        ResiliencePolicy = Policy
            .Handle<NpgsqlException>()
            .Or<DbException>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(10),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(500)
            });
    }

    private UnitOfWorkPostgres(string connectionString, CancellationToken cancellationToken)
        : base(Guid.NewGuid().ToString(), new ConnectionManager(NpgsqlFactory.Instance, connectionString, ResiliencePolicy), cancellationToken)
    {
    }

    public static IUnitOfWork Create(IConnectionManager connectionManager, CancellationToken cancellationToken)
    {
        return new UnitOfWorkPostgres(connectionManager.ConnectionString, cancellationToken);
    }
}