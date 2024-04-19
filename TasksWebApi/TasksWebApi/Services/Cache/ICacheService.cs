namespace TasksWebApi.Services;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key, T defaultValue = default(T), CancellationToken cancellationToken = default);
    Task SetWithDefaultExpirationAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, DateTime absoluteExpiration, TimeSpan slidingExpiration,  CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, DateTime absoluteExpiration,  CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration,  CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}