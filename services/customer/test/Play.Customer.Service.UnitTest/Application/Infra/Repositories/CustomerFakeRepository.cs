namespace Play.Customer.Service.UnitTest.Application.Infra.Repositories;

using System.Threading;
using System.Threading.Tasks;
using Core.Domain.AggregateModel.CustomerAggregate;
using CSharpFunctionalExtensions;

public sealed class CustomerFakeRepository : ICustomerRepository
{
    public Task<Maybe<Customer>> GetByDocumentAsync(string document)
    {
        throw new System.NotImplementedException();
    }

    public Task<Maybe<Customer>> GetByEmailAsync(string email)
    {
        throw new System.NotImplementedException();
    }

    public Task<Maybe<Customer>> GetByIdAsync(string id)
    {
        throw new System.NotImplementedException();
    }

    public Task SaveAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}