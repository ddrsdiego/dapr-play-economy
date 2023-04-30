namespace Play.Customer.Service;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class Program
{
    private const string AspnetcoreEnvironment = "ASPNETCORE_ENVIRONMENT";

    public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(cb =>
            {
                var env = Environment.GetEnvironmentVariable(AspnetcoreEnvironment) ?? "Development";
                var appSettingsJson = $"appsettings.{env}.json";
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(appSettingsJson, false, true)
                    .AddEnvironmentVariables()
                    .Build();
                cb.AddConfiguration(configuration);
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}