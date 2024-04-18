namespace TasksWebApi.Services;

public record JwtSettings(string Issuer, string Audience, string Key, int ExpireMinutes, int RefreshTokenExpireMinutes)
{
    public static JwtSettings FromDictionary(IDictionary<string, object> dictionary)
    {
        return new JwtSettings(
            dictionary["Issuer"].ToString(),
            dictionary["Audience"].ToString(),
            dictionary["Key"].ToString(),
            int.Parse(dictionary["ExpireMinutes"].ToString()!),
            int.Parse(dictionary["RefreshTokenExpireMinutes"].ToString()!)
        );
    }
}