namespace TasksWebApi.Services;

public class AppSettingsConfigurationValuesService(IConfiguration configuration) : IConfigurationValuesService
{
    public Task<string> GetDataBaseConnection()
    {
        var connectionString = configuration.GetConnectionString("DataBaseConnection");
        return Task.FromResult(connectionString);
    }

    public Task<string> GetXApiKey()
    {
        var connectionString = configuration.GetValue<string>("XApiKey");
        return Task.FromResult(connectionString);
    }

    public Task<JwtSettings> GetJwtSettings()
    {
        return Task.FromResult(new JwtSettings(
            configuration.GetValue<string>("Jwt:Issuer"),
            configuration.GetValue<string>("Jwt:Audience"),
            configuration.GetValue<string>("Jwt:Key"),
            configuration.GetValue<int>("Jwt:ExpireMinutes"),
            configuration.GetValue<int>("Jwt:RefreshTokenExpireMinutes")
        ));
    }

    public Task<AuditSettings> GetAuditSettings()
    {
        return Task.FromResult(new AuditSettings(
            configuration.GetValue<LogType>("Audit:Type"),
            configuration.GetValue<string>("Audit:ConnectionString"),
            configuration.GetValue<string>("Audit:TableName")
        ));
    }

    public Task<SerilogSettings> GetSerilogSettings()
    {
        return Task.FromResult(new SerilogSettings(
            configuration.GetValue<LogType>("SerilogLog:Type"),
            configuration.GetValue<string>("SerilogLog:ConnectionString"),
            configuration.GetValue<string>("SerilogLog:TableName")
        ));
    }
}