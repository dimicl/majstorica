using System.Text.Json;
using System.Text.Json.Serialization;
using backend.Application.Interfaces;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisJsonStringCache : IRedisJsonCache
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public RedisJsonStringCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return _db.StringSetAsync(key, json, ttl);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _db.KeyDeleteAsync(key);
}
