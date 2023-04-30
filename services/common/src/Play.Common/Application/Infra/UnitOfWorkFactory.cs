namespace Play.Common.Application.Infra;

using System.Threading;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly string _connectionString;

    public UnitOfWorkFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IUnitOfWork Create(CancellationToken cancellationToken = default)
    {
        return UnitOfWorkPostgres.Create(_connectionString, cancellationToken);
    }
}