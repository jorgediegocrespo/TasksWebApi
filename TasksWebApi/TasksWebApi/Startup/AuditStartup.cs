using Audit.Core;
using Audit.WebApi;
using Microsoft.AspNetCore.Mvc;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class AuditStartup
{
    public static void AuditSetupFilter(this MvcOptions mvcOptions, IServiceCollection services)
    {
        var configurationValues = services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        var auditSettings = configurationValues.GetAuditSettingsAsync().Result;
        if (auditSettings.Type == LogType.None)
            return;
        
        mvcOptions.AddAuditFilter(a => a
            .LogAllActions()
            .WithEventType("{controller} {action} {verb}")
            .IncludeModelState()
            .IncludeRequestBody()
            .IncludeResponseBody());
    }

    public static void UseAudit(this WebApplication app, IServiceCollection services)
    {
        var configurationValues = services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        var auditSettings = configurationValues.GetAuditSettingsAsync().Result;
        if (auditSettings.Type == LogType.None)
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
        
        app.AuditSetupOutput(auditSettings);
    }
    
    private static void AuditSetupOutput(this WebApplication app, AuditSettings auditSettings)
    {
        switch (auditSettings.Type)
        {
            case LogType.None:
                break;
            case LogType.Custom:
                app.AuditSetupCustomOutput();
                break;
            case LogType.AzureTableStorage:
                app.AuditSetupAzureTableStorageOutput(auditSettings);
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
    
    private static void AuditSetupAzureTableStorageOutput(this WebApplication app, AuditSettings auditSettings)
    {
        Configuration.Setup()
            .UseAzureTableStorage(_ => _
                .ConnectionString(auditSettings.ConnectionString)
                .TableName(auditSettings.TableName)
                .EntityBuilder(e => e
                    .PartitionKey(ev => $"{auditSettings.TableName}{ev.GetWebApiAuditAction().UserName}{ev.StartDate:yyyyMM}")
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