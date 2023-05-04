namespace Play.Common.Application.Infra.Repositories.Dapr;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

public interface IDaprStateEntryRepository<TEntry>
    where TEntry : IDaprStateEntry
{
    Task UpsertAsync(TEntry entity, CancellationToken cancellationToken = default);

    Task UpsertAsync(TEntry[] entities, CancellationToken cancellationToken = default);

    Task<Result<TEntry>> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, Result<TEntry>>> GetCustomerByIdAsync(string[] ids,
        CancellationToken cancellationToken = default);
}