namespace Play.Common.Application.Infra.UoW;

using System.Threading;
using System.Threading.Tasks;
using Repositories;

public interface IUnitOfWorkFactory
{
    /// <summary>
    /// Create a new unit of work.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IConnectionManager _connectionManager;

    public UnitOfWorkFactory(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default)
    {
        var unitOfWork = UnitOfWorkPostgres.Create(_connectionManager, cancellationToken);
        await unitOfWork.BeginTransactionAsync();

        return unitOfWork;
    }
}