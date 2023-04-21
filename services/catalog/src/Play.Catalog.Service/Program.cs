namespace Play.Catalog.Service
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Common.Api;
    using Core.Application.IoC;
    using Core.Application.UseCases.CreateNewCatalogItem;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public abstract class Program
    {
        public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilogCustom()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureServices((context, services) =>
                {
                    services.AddMediatR(cfg =>
                        cfg.RegisterServicesFromAssemblyContaining<CreateNewCatalogItemCommandHandler>());

                    services.AddSwaggerVersioning();
                    services.AddDaprStateEntryRepositories();
                    services.AddDaprClient(configure => configure.UseJsonSerializationOptions(new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    }));
                    services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = false);
                });
    }
}