namespace TasksWebApi.Services;

public interface IConfigurationValuesService
{
    Task<string> GetDataBaseConnection();
    Task<string> GetXApiKey();
    Task<JwtSettings> GetJwtSettings();
    Task<AuditSettings> GetAuditSettings();
    Task<SerilogSettings> GetSerilogSettings();
}