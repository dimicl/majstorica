using System.Text.Json;
using backend.Application.Interfaces;
using backend.Api.DTOs.Master;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisMastersListCache : IRedisListCache
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RedisMastersListCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<List<MasterListItemResponse>?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<MasterListItemResponse>>(value!, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetAsync(string key, List<MasterListItemResponse> list, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(list, JsonOptions);
        await _db.StringSetAsync(key, json, ttl);
    }
}
