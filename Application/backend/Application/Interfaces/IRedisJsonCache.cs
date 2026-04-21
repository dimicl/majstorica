namespace backend.Application.Interfaces;

/// <summary>Generički string (JSON) keš u Redisu — poslovi, firme, itd.</summary>
public interface IRedisJsonCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
