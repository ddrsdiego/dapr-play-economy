namespace Play.Customer.Core.Application.Infra.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Infra.Repositories;
using CSharpFunctionalExtensions;
using Dapper;
using Domain.AggregateModel.CustomerAggregate;
using Microsoft.Extensions.Logging;
using Statements;

public sealed class CustomerRepository : Repository, ICustomerRepository
{
    private readonly ILogger<CustomerRepository> _logger;
    private readonly IConnectionManager _connectionManager;

    public CustomerRepository(ILogger<CustomerRepository> logger, IConnectionManager connectionManager)
        : base(logger, connectionManager)
    {
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public async Task<Maybe<Customer>> GetByEmailAsync(string email)
    {
        try
        {
            await using var conn = await _connectionManager.GetOpenConnectionAsync();
            var customerData = await conn.QueryFirstOrDefaultAsync<CustomerData>(
                CustomerRepositoryStatement.GetByEmailAsync,
                new
                {
                    Email = email
                });

            return customerData == null ? Maybe<Customer>.None : Maybe<Customer>.From(customerData.ToCustomer());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Maybe<Customer>> GetByIdAsync(string id)
    {
        try
        {
            CustomerData customerData;
            
            await using (var conn = await _connectionManager.GetOpenConnectionAsync())
            {
                customerData = await conn.QueryFirstOrDefaultAsync<CustomerData>(CustomerRepositoryStatement.GetByIdAsync,
                    new
                    {
                        CustomerId = id
                    });
            }

            return customerData == null ? Maybe<Customer>.None : Maybe.From(customerData.ToCustomer());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task SaveAsync(Customer aggregateRoot, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = aggregateRoot.ToCustomerData();
            await using var conn = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            
            await conn.ExecuteAsync(CustomerRepositoryStatement.SaveAsync,
                new
                {
                    data.CustomerId,
                    data.Document,
                    data.Email,
                    data.Name,
                    data.CreatedAt
                });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "");
            throw;
        }
    }

    public async Task UpdateAsync(Customer aggregateRoot, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = aggregateRoot.ToCustomerData();

            await using var conn  = await _connectionManager.GetOpenConnectionAsync(cancellationToken);
            await conn.ExecuteAsync(CustomerRepositoryStatement.UpdateAsync,
                new {data.CustomerId, data.Name});
        }
        catch (Exception e)
        {
            _logger.LogError(e, "");
        }
    }

    public async Task<Maybe<Customer>> GetByDocumentAsync(string document)
    {
        try
        {
            var documentResult = Document.Create(document);

            await using var conn = await _connectionManager.GetOpenConnectionAsync();
            var customerData = await conn.QueryFirstOrDefaultAsync<CustomerData>(
                CustomerRepositoryStatement.GetByDocumentAsync,
                new
                {
                    Document = documentResult.Value.Value
                });

            return customerData == null ? Maybe<Customer>.None : Maybe<Customer>.From(customerData.ToCustomer());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}