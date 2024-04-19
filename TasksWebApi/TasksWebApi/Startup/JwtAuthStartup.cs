using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class JwtAuthStartup
{
    public static void AddJwtAuthentication(this IServiceCollection services)
    {
        var configurationValues = services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o => o.TokenValidationParameters = GetTokenValidationParameters(configurationValues));
    }

    private static TokenValidationParameters GetTokenValidationParameters(IConfigurationValuesService configurationValues)
    {
        var jwtSettings = configurationValues.GetJwtSettingsAsync().Result;
        return new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(jwtSettings.ExpireMinutes),
        };
    }
}