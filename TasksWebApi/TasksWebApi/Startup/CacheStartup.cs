using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class CacheStartup
{
    public static void AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICache, RedisCacheService>();
        services.AddStackExchangeRedisCache(options => { options.Configuration = configuration["RedisCache:Url"]; });
    }
}