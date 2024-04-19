namespace TasksWebApi.Services;

public class NoneCacheService : ICacheService
{
    public Task<T> GetAsync<T>(string key, T defaultValue = default(T), CancellationToken cancellationToken = default)
    {
        return Task.FromResult(defaultValue);
    }

    public Task SetWithDefaultExpirationAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, DateTime absoluteExpiration, TimeSpan slidingExpiration,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}