using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TasksWebApi.Startup;

public static class TelemetryStartup
{
    public static void AddOpenTelemetry(this IServiceCollection serviceCollection, IWebHostEnvironment currentEnvironment, IConfiguration configuration)
    {
        var useTelemetry = configuration.GetValue<bool>("OpenTelemetry:UseTelemetry");
        if (!useTelemetry)
            return;
        
        serviceCollection.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddSource(currentEnvironment.ApplicationName)
                .ConfigureResource(resource => resource
                    .AddService(currentEnvironment.ApplicationName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddMeter(currentEnvironment.ApplicationName)
                .ConfigureResource(resource => resource
                    .AddService(currentEnvironment.ApplicationName))
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddProcessInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEventCountersInstrumentation(c =>
                {
                    // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                    c.AddEventSources(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft-AspNetCore-Server-Kestrel",
                        "System.Net.Http",
                        "System.Net.Sockets");
                })
                .AddOtlpExporter());
    }
}