using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class CacheStartup
{
    public static void AddCache(this IServiceCollection services)
    {
        var configurationValues = services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        var redisSettings = configurationValues.GetRedisSettingsAsync().Result;
        if (string.IsNullOrWhiteSpace(redisSettings.ConnectionString))
        {
            services.AddSingleton<ICacheService, NoneCacheService>();
            return;
        }
        
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisSettings.ConnectionString;
            options.InstanceName = redisSettings.InstanceName;
        });
    }
}