using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace TasksWebApi.Services;

public class RedisCacheService(IDistributedCache cache, IConfiguration configuration, ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly TimeSpan defaultAbsoluteExpiration = TimeSpan.Parse(configuration["RedisCache:DefaultAbsoluteExpiration"]!);
    private readonly TimeSpan defaultSlidingExpiration = TimeSpan.Parse(configuration["RedisCache:DefaultSlidingExpiration"]!);

    public async Task<T> GetAsync<T>(string key, T defaultValue = default, CancellationToken cancellationToken = default)
    {
        if (cache == null)
            return defaultValue;

        try
        {
            var data = await cache.GetStringAsync(key, cancellationToken);
            return data == null ? defaultValue : JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting data from cache");
            return defaultValue;
        }
    }
    
    public async Task SetWithDefaultExpirationAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        if (cache == null)
            return;

        try
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                //Sets a specific date and time when the cache entry will expire, regardless of whether it has been accessed recently or not.
                .SetAbsoluteExpiration(DateTime.UtcNow.Add(defaultAbsoluteExpiration))
                //Sets a time interval during which the cache entry remains valid, and the expiration timer resets each time the cache entry is accessed
                .SetSlidingExpiration(defaultSlidingExpiration);
        
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting data from cache");
        }
    }
    
    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        if (cache == null)
            return;
        
        try
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting data from cache");
        }
    }

    public async Task SetAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default)
    {
        if (cache == null)
            return;
        
        try
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(absoluteExpiration);
        
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting data from cache");
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration, CancellationToken cancellationToken = default)
    {
        if (cache == null)
            return;
        
        try
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(slidingExpiration);
        
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting data from cache");
        }
    }

    public async Task SetAsync<T>(string key, T value, DateTime absoluteExpiration, TimeSpan slidingExpiration, CancellationToken cancellationToken = default)
    {
        if (cache == null)
            return;
        
        try
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(absoluteExpiration)
                .SetSlidingExpiration(slidingExpiration);
        
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting data from cache");
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (cache == null)
            return;
        
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting data from cache");
        }
    }
}