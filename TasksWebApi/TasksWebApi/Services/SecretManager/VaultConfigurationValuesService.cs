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

    public async Task<string> GetDataBaseConnection()
    {
        var returnedData = await Get("ConnectionStrings");
        return returnedData["DataBaseConnection"].ToString();
    }

    public async Task<string> GetXApiKey()
    {
        var returnedData = await Get("XApiKey");
        return returnedData["Value"].ToString();
    }

    public async Task<JwtSettings> GetJwtSettings()
    {
        var returnedData = await Get("Jwt");
        return JwtSettings.FromDictionary(returnedData);
    }

    public async Task<AuditSettings> GetAuditSettings()
    {
        var returnedData = await Get("Audit");
        return AuditSettings.FromDictionary(returnedData);
    }

    public async Task<SerilogSettings> GetSerilogSettings()
    {
        var returnedData = await Get("SerilogLog");
        return SerilogSettings.FromDictionary(returnedData);
    }

    private async Task<IDictionary<string, object>> Get(string path)
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