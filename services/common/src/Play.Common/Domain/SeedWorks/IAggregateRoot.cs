namespace Play.Common.Domain.SeedWorks
{
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;

    public interface IAggregateRoot
    {
    }

    public interface IRepository<T> where T : IAggregateRoot
    {
        Task<Maybe<T>> GetByIdAsync(string id);

        Task SaveAsync(T aggregateRoot, CancellationToken cancellationToken = default);

        Task UpdateAsync(T aggregateRoot, CancellationToken cancellationToken = default);
    }
}