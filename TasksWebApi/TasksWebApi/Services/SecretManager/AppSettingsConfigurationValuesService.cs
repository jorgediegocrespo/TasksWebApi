namespace TasksWebApi.Services;

public class AppSettingsConfigurationValuesService(IConfiguration configuration) : IConfigurationValuesService
{
    public Task<string> GetDataBaseConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DataBaseConnection");
        return Task.FromResult(connectionString);
    }

    public Task<string> GetXApiKeyAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetValue<string>("XApiKey");
        return Task.FromResult(connectionString);
    }

    public Task<JwtSettings> GetJwtSettingsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new JwtSettings(
            configuration.GetValue<string>("Jwt:Issuer"),
            configuration.GetValue<string>("Jwt:Audience"),
            configuration.GetValue<string>("Jwt:Key"),
            configuration.GetValue<int>("Jwt:ExpireMinutes"),
            configuration.GetValue<int>("Jwt:RefreshTokenExpireMinutes")
        ));
    }

    public Task<AuditSettings> GetAuditSettingsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AuditSettings(
            configuration.GetValue<LogType>("Audit:Type"),
            configuration.GetValue<string>("Audit:ConnectionString"),
            configuration.GetValue<string>("Audit:TableName")
        ));
    }

    public Task<SerilogSettings> GetSerilogSettingsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SerilogSettings(
            configuration.GetValue<LogType>("SerilogLog:Type"),
            configuration.GetValue<string>("SerilogLog:ConnectionString"),
            configuration.GetValue<string>("SerilogLog:TableName")
        ));
    }
    
    public Task<RedisSettings> GetRedisSettingsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RedisSettings(
            configuration.GetValue<string>("RedisCache:ConnectionString"),
            configuration.GetValue<string>("RedisCache:InstanceName")
        ));
    }
}