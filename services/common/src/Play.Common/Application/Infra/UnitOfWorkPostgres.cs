namespace Play.Common.Application.Infra;

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
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

    public static IUnitOfWork Create(string connectionString, CancellationToken cancellationToken)
    {
        var uow = new UnitOfWorkPostgres(connectionString, cancellationToken);
        uow.BeginTransaction();

        return uow;
    }

    private UnitOfWorkPostgres(string connectionString, CancellationToken cancellationToken)
        : base(new ConnectionManager(NpgsqlFactory.Instance, connectionString, ResiliencePolicy), cancellationToken)
    {
    }

    public override Task OpenAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionManager.GetOpenConnectionAsync(cancellationToken);
    }

    public override Task CloseAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionManager.CloseAsync(cancellationToken);
    }

    public override Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionManager.TransactionManager.CommitAsync(cancellationToken);
    }

    public override Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionManager.TransactionManager.RollbackAsync(cancellationToken);
    }

    public override Task SaveChangesAsyncAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask; // implementar a lógica de salvar as alterações no banco de dados aqui
    }
}