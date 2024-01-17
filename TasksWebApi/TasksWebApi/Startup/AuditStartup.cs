using Audit.Core;
using Audit.WebApi;
using Microsoft.AspNetCore.Mvc;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class AuditStartup
{
    public static void AuditSetupFilter(this MvcOptions mvcOptions, IConfiguration configuration)
    {
        var auditProviderType = configuration.GetValue<LogType>("Audit:Type");
        if (auditProviderType == LogType.None)
            return;
        
        mvcOptions.AddAuditFilter(a => a
            .LogAllActions()
            .WithEventType("{controller} {action} {verb}")
            .IncludeModelState()
            .IncludeRequestBody()
            .IncludeResponseBody());
    }

    public static void UseAudit(this WebApplication app, IConfiguration configuration)
    {
        var auditProviderType = configuration.GetValue<LogType>("Audit:Type");
        if (auditProviderType == LogType.None)
            return;
        
        app.Use(async (context, next) => {
            context.Request.EnableBuffering();
            await next();
        });
        
        app.UseAuditMiddleware(x => x
            .IncludeHeaders()
            .IncludeResponseHeaders()
            .IncludeRequestBody()
            .IncludeResponseBody()
        );
        
        app.AuditSetupOutput(configuration);
    }
    
    private static void AuditSetupOutput(this WebApplication app, IConfiguration configuration)
    {
        var auditProviderType = configuration.GetValue<LogType>("Audit:Type");
        switch (auditProviderType)
        {
            case LogType.None:
                break;
            case LogType.Custom:
                app.AuditSetupCustomOutput();
                break;
            case LogType.AzureTableStorage:
                app.AuditSetupAzureTableStorageOutput(configuration);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void AuditSetupCustomOutput(this WebApplication app)
    {
        Configuration.Setup().UseCustomProvider(new CustomAuditDataProvider());
        Configuration.JsonSettings.WriteIndented = true;
    }
    
    private static void AuditSetupAzureTableStorageOutput(this WebApplication app, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("Audit:ConnectionString");
        var tableName = configuration.GetValue<string>("Audit:TableName");
        
        Configuration.Setup()
            .UseAzureTableStorage(_ => _
                .ConnectionString(connectionString)
                .TableName(tableName)
                .EntityBuilder(e => e
                    .PartitionKey(ev => $"{tableName}{ev.GetWebApiAuditAction().UserName}{ev.StartDate:yyyyMM}")
                    .RowKey(_ => Guid.NewGuid().ToString())
                    .Columns(c => c.FromObject(ev => new
                    {
                        Date = ev.StartDate,
                        Controller = ev.GetWebApiAuditAction().ControllerName,
                        Action = ev.GetWebApiAuditAction().ActionName,
                        Method = ev.GetWebApiAuditAction().HttpMethod,
                        Result = ev.GetWebApiAuditAction().ResponseStatusCode,
                        AuditEventJson = ev.ToJson(),
                        ev.Duration,
                        ev.GetWebApiAuditAction()?.UserName,
                    }))));
    }
}