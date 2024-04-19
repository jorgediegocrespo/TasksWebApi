using Azure.Data.Tables;
using HealthChecks.Azure.Data.Tables;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class HealthChecksStartup
{
    public static void AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var configurationValues = services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        
        var sqlServerConnectionString = configurationValues.GetDataBaseConnectionAsync().Result;
        var redisConnectionString = configuration.GetValue<string>("RedisCache:ConnectionString");
        
        var serilogSettings = configurationValues.GetSerilogSettingsAsync().Result;
        var serilogProviderType = serilogSettings.Type;
        var serilogAzureTableStorageConnectionString = serilogSettings.ConnectionString;
        var serilogAzureTableName = serilogSettings.TableName;
        
        var auditSettings = configurationValues.GetAuditSettingsAsync().Result;
        var auditProviderType = auditSettings.Type;
        var auditAzureTableStorageConnectionString = auditSettings.ConnectionString;
        var auditAzureTableName = auditSettings.TableName;

        var healthChecksBuilder = services.AddHealthChecks()
            .AddSqlServer(sqlServerConnectionString!);

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            healthChecksBuilder = healthChecksBuilder
                .AddRedis(redisConnectionString);
        }

        if (serilogProviderType == LogType.AzureTableStorage)
        {
            healthChecksBuilder = healthChecksBuilder
                .AddAzureTable(_ => new TableServiceClient(serilogAzureTableStorageConnectionString),
                    _ => new AzureTableServiceHealthCheckOptions
                    {
                        TableName = serilogAzureTableName
                    }, name: "SerilogAzureTableStorage");
        }

        if (auditProviderType == LogType.AzureTableStorage)
        {
            healthChecksBuilder = healthChecksBuilder
                .AddAzureTable(_ => new TableServiceClient(auditAzureTableStorageConnectionString),
                    _ => new AzureTableServiceHealthCheckOptions
                    {
                        TableName = auditAzureTableName
                    }, name: "AuditAzureTableStorage");
        }

        services.AddHealthChecksUI().AddInMemoryStorage();
    }
    
    public static void MapCustomHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        app.UseHealthChecksUI(config =>
        {
            config.UIPath = "/health-ui";            
        });
    }
}