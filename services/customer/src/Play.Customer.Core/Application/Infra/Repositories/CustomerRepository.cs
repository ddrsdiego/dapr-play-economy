namespace Play.Customer.Core.Application.Infra.Repositories
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Dapr.Client;
    using Domain.AggregateModel.CustomerAggregate;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Statements;

    public sealed class CustomerRepository : Repository, ICustomerRepository
    {
        private readonly AppSettings _appSettings;
        private readonly DaprClient _daprClient;
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(ILogger<CustomerRepository> logger, DaprClient daprClient,
            IOptions<AppSettings> options, IOptions<ConnectionStringOptions> connectionStringOptions,
            IDbConnection dbConnection)
            : base(logger, connectionStringOptions)
        {
            _logger = logger;
            _daprClient = daprClient;
            _dbConnection = dbConnection;
            _appSettings = options.Value;
        }

        public async Task<Customer> GetByIdAsync(string customerId)
        {
            try
            {
                using var conn = GetConnection();
                var customerData = await conn.QueryFirstOrDefaultAsync<CustomerData>(
                    CustomerRepositoryStatement.GetByIdAsync,
                    new
                    {
                        CustomerId = customerId
                    });

                return customerData == null ? Customer.Default : customerData.ToCustomer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            try
            {
                using var conn = GetConnection();
                var customerData = await conn.QueryFirstOrDefaultAsync<CustomerData>(
                    CustomerRepositoryStatement.GetByEmailAsync,
                    new
                    {
                        Email = email
                    });

                return customerData == null ? Customer.Default : customerData.ToCustomer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task SaveAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var data = customer.ToCustomerData();

                using var conn = GetConnection();
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

            static async Task SlowExecute(Task task) => await task;
        }

        public async Task<Customer> GetByDocumentAsync(string document)
        {
            try
            {
                using var conn = GetConnection();
                var customerData = await conn.QueryFirstOrDefaultAsync<CustomerData>(
                    CustomerRepositoryStatement.GetByDocumentAsync,
                    new
                    {
                        Document = document
                    });

                return customerData == null ? Customer.Default : customerData.ToCustomer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<Customer> GetByFormattedKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var formattedKey = CustomerData.GetKeyFormatted(key);

            try
            {
                var state = await _daprClient.GetStateEntryAsync<CustomerData>(_appSettings.DaprSettings.StateStoreName,
                    formattedKey);

                return state.Value == null ? Customer.Default : state.Value.ToCustomer();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                throw;
            }
        }

        public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = customer.ToCustomerData();

                using var conn = GetConnection();
                await conn.ExecuteAsync(CustomerRepositoryStatement.UpdateAsync,
                    new
                    {
                        data.CustomerId,
                        data.Name,
                    });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                throw;
            }

            static async Task SlowExecute(Task task) => await task;
        }
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
}