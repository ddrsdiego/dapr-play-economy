namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate;

using System;
using System.Threading.Tasks;
using Common.Domain.SeedWorks;
using CSharpFunctionalExtensions;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Maybe<Customer>> GetByEmailAsync(string email);
    
    Task<Maybe<Customer>> GetByDocumentAsync(string document);
}

internal sealed class CustomerData
{
    public string Id { get; set; }
    public string CustomerId { get; set; }
    public string Document { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static string GetKeyFormatted(string value) => $"{nameof(Customer).ToLowerInvariant()}-{value}";
}