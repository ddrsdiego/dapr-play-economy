namespace Play.Customer.Service
{
    using Common.Api;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Core.Application.IoC;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Configuration;
    using Subscribers.v1;

    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAppBaseConfig();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddPlayCustomerServices(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseCloudEvents();
            app.UseSwaggerVersioning(apiVersionDescriptionProvider);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.HandleCustomerRegistered();
                endpoints.MapSubscribeHandler();
            });
        }
    }
}