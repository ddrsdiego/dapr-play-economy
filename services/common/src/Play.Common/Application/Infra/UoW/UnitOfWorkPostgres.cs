namespace Play.Common.Application.Infra.UoW;

using System;
using System.Data.Common;
using Npgsql;
using Polly;
using Repositories;

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

    private UnitOfWorkPostgres(ITransactionManagerFactory transactionFactory)
        : base(GeneratorOperationId.Generate(), transactionFactory)
    {
    }

    private UnitOfWorkPostgres(string unitOfWorkContextId, ITransactionManagerFactory transactionManagerFactory)
        : base(unitOfWorkContextId, transactionManagerFactory)
    {
    }

    public static IUnitOfWork Create(ITransactionManagerFactory transactionManagerFactory) => new UnitOfWorkPostgres(Guid.NewGuid().ToString(), transactionManagerFactory);

    public static IUnitOfWork Create(string unitOfWorkContextId, ITransactionManagerFactory transactionManagerFactory) => new UnitOfWorkPostgres(unitOfWorkContextId, transactionManagerFactory);
}