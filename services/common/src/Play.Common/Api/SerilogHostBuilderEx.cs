namespace Play.Common.Api;

using System;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;

public static class SerilogHostBuilderEx
{
    public static IHostBuilder UseSerilogCustom(this IHostBuilder hostBuilder)
    {
        var assembly = Assembly.GetExecutingAssembly().GetName();
            
        hostBuilder.UseSerilogCustom(configurator =>
        {
            configurator.ServiceName = assembly.Name;
            configurator.SquadName = "Foundation";
            configurator.ServiceVersion = assembly.Version.Build.ToString();
        });
        return hostBuilder;
    }
        
    public static IHostBuilder UseSerilogCustom(this IHostBuilder hostBuilder,
        Action<SerilogBuilderConfigurator> configurator)
    {
        const string assemblyParameter = "Assembly";
        const string squadNameParameter = "Jornada";
            
        var serilogBuilder = new SerilogBuilderConfigurator();
        configurator(serilogBuilder);
            
        hostBuilder.UseSerilog((context, provider, logger) =>
        {
            logger
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Destructure.ToMaximumCollectionCount(10)
                .Destructure.ToMaximumStringLength(1024)
                .Destructure.ToMaximumDepth(5)
                .Enrich.WithProperty(squadNameParameter, serilogBuilder.SquadName)
                .Enrich.WithProperty(assemblyParameter, serilogBuilder.ServiceName)
                .Enrich.WithProperty("Version", serilogBuilder.ServiceVersion)
                .Enrich
                .WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithRootName("Exception"));
        });
        return hostBuilder;
    }
}

public class SerilogBuilderConfigurator
{
    public string SquadName { get; set; }
    public string ServiceName { get; set; }
    public string ServiceVersion { get; set; }
}