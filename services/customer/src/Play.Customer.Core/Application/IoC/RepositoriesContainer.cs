namespace Play.Customer.Core.Application.IoC
{
    using System.Data;
    using Common.Application.UseCase;
    using Domain.AggregateModel.CustomerAggregate;
    using Infra.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Npgsql;

    internal static class RepositoriesContainer
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICustomerRepository, CustomerRepository>();
            services.AddSingleton<IRequestManager, RequestManagerMemoryCache>();
            services.TryAddSingleton<IDbConnection>(sp =>
            {
                var connectionString = configuration.GetSection("ConnectionStringOptions:PostgresConnection").Value;
                return new NpgsqlConnection(connectionString);
            });

            return services;
        }
    }
}