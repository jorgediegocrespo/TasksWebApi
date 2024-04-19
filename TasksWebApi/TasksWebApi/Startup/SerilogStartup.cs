using Serilog;
using Serilog.Events;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class SerilogStartup
{
    public static void SetupSerilog(this WebApplicationBuilder builder)
    {
        var configurationValues = builder.Services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        var serilogSettings = configurationValues.GetSerilogSettingsAsync().Result;
        if (serilogSettings.Type == LogType.None)
            return;
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u1}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.AzureTableStorage(
                serilogSettings.ConnectionString, 
                storageTableName: serilogSettings.TableName,
                propertyColumns: new[] { "SourceContext", "RequestId", "RequestPath", "ConnectionId" })
            .CreateLogger();
    }
    
    public static void UseSerilog(this WebApplicationBuilder builder)
    {
        var configurationValues = builder.Services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        var serilogSettings = configurationValues.GetSerilogSettingsAsync().Result;
        if (serilogSettings.Type == LogType.None)
            return;
        
        builder.Host.UseSerilog();
    }
}