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

    private UnitOfWorkPostgres(IConnectionManager connectionManager, CancellationToken cancellationToken)
        : base(Guid.NewGuid().ToString(), connectionManager, cancellationToken)
    {
    }

    public static IUnitOfWork Create(IConnectionManager connectionManager, CancellationToken cancellationToken) => new UnitOfWorkPostgres(connectionManager, cancellationToken);
}