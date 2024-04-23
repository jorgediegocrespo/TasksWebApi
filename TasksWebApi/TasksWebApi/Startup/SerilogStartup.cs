using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class SerilogStartup
{
    public static void SetupOpenTelemetrySerilog(this WebApplicationBuilder builder, IWebHostEnvironment currentEnvironment)
    {
        const string outputTemplate =
            "[{Level:w}]: {Timestamp:dd-MM-yyyy:HH:mm:ss} {MachineName} {EnvironmentName} {SourceContext} {Message}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.OpenTelemetry(opts =>
            {
                opts.ResourceAttributes = new Dictionary<string, object>
                {
                    ["app"] = currentEnvironment.ApplicationName,
                    ["runtime"] = "dotnet",
                    ["service.name"] = currentEnvironment.ApplicationName
                };
            })
            .CreateLogger();
    }
    
    public static void UseOpenTelemetrySerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
    }
}