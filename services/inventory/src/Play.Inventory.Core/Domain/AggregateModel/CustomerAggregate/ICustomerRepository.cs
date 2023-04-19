namespace Play.Inventory.Core.Domain.AggregateModel.CustomerAggregate
{
    public interface ICustomerRepository
    {
        Task<Customer> GetCustomerByIdAsync(string userId);
        Task UpsertAsync(Customer customer);
    }
}