using Microsoft.Extensions.Options;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace TasksWebApi.Services;

public class VaultConfigurationValuesService : IConfigurationValuesService
{
    private readonly VaultSettings _vaultSettings;

    public VaultConfigurationValuesService(IOptions<VaultSettings> vaultSettings)
    {
        _vaultSettings = vaultSettings.Value with { TokenApi = GetTokenFromEnvironmentVariable() };
    }

    public async Task<string> GetDataBaseConnectionAsync(CancellationToken cancellationToken = default)
    {
        var returnedData = await GetAsync("ConnectionStrings", cancellationToken);
        return returnedData["DataBaseConnection"].ToString();
    }

    public async Task<string> GetXApiKeyAsync(CancellationToken cancellationToken = default)
    {
        var returnedData = await GetAsync("XApiKey", cancellationToken);
        return returnedData["Value"].ToString();
    }

    public async Task<JwtSettings> GetJwtSettingsAsync(CancellationToken cancellationToken = default)
    {
        var returnedData = await GetAsync("Jwt", cancellationToken);
        return JwtSettings.FromDictionary(returnedData);
    }

    public async Task<AuditSettings> GetAuditSettingsAsync(CancellationToken cancellationToken = default)
    {
        var returnedData = await GetAsync("Audit", cancellationToken);
        return AuditSettings.FromDictionary(returnedData);
    }

    public async Task<SerilogSettings> GetSerilogSettingsAsync(CancellationToken cancellationToken = default)
    {
        var returnedData = await GetAsync("SerilogLog", cancellationToken);
        return SerilogSettings.FromDictionary(returnedData);
    }
    
    public async Task<RedisSettings> GetRedisSettingsAsync(CancellationToken cancellationToken = default)
    {
        var returnedData = await GetAsync("Redis", cancellationToken);
        return RedisSettings.FromDictionary(returnedData);
    }

    private async Task<IDictionary<string, object>> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        VaultClient client = new VaultClient(new VaultClientSettings(_vaultSettings.VaultUrl, new TokenAuthMethodInfo(_vaultSettings.TokenApi)));
        Secret<SecretData> kv2Secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: path, mountPoint: "secret");
        return kv2Secret.Data.Data;
    }

    private string GetTokenFromEnvironmentVariable()
    {
        return Environment.GetEnvironmentVariable("VAULT-TOKEN") ?? throw new KeyNotFoundException("vault is not implemented into the system");
    }
}