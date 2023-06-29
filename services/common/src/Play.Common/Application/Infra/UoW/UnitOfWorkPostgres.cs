namespace Play.Common.Application.Infra.UoW;

using System;
using System.Data.Common;
using System.Threading;
using Npgsql;
using Play.Common;
using Play.Common.Application.Infra.Repositories;
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
        : base(GeneratorOperationId.Generate(), new ConnectionManager(NpgsqlFactory.Instance, connectionString, ResiliencePolicy), cancellationToken)
    {
    }

    private UnitOfWorkPostgres(string unitOfWorkContextId, string connectionString, CancellationToken cancellationToken)
        : base(unitOfWorkContextId, new ConnectionManager(NpgsqlFactory.Instance, connectionString, ResiliencePolicy), cancellationToken)
    {
    }

    public static IUnitOfWork Create(IConnectionManager connectionManager, CancellationToken cancellationToken) =>
        new UnitOfWorkPostgres(Guid.NewGuid().ToString(), connectionManager.ConnectionString, cancellationToken);

    public static IUnitOfWork Create(string unitOfWorkContextId, IConnectionManager connectionManager, CancellationToken cancellationToken) =>
        new UnitOfWorkPostgres(unitOfWorkContextId, connectionManager.ConnectionString, cancellationToken);
}