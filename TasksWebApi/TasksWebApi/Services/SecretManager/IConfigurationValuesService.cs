namespace TasksWebApi.Services;

public interface IConfigurationValuesService
{
    Task<string> GetDataBaseConnectionAsync(CancellationToken cancellationToken = default);
    Task<string> GetXApiKeyAsync(CancellationToken cancellationToken = default);
    Task<JwtSettings> GetJwtSettingsAsync(CancellationToken cancellationToken = default);
    Task<AuditSettings> GetAuditSettingsAsync(CancellationToken cancellationToken = default);
    Task<SerilogSettings> GetSerilogSettingsAsync(CancellationToken cancellationToken = default);
    Task<RedisSettings> GetRedisSettingsAsync(CancellationToken cancellationToken = default);
}