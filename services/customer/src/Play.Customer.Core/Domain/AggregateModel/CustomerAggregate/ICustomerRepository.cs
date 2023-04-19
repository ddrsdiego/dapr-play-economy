namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(string customerId);

        Task<Customer> GetByDocumentAsync(string document);
        
        Task<Customer> GetByEmailAsync(string email);

        Task SaveAsync(Customer customer, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    }
}