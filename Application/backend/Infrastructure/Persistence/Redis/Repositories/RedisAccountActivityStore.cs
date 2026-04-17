using System.Text.Json;
using backend.Application.Interfaces;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisAccountActivityStore : IAccountActivityStore
{
    private readonly IDatabase _db;
    private const int MaxEntriesPerUser = 100;
    private static readonly TimeSpan ListTtl = TimeSpan.FromDays(90);

    private static string ListKey(Guid userId) => $"activity:user:{userId:D}";

    public RedisAccountActivityStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task RecordAsync(
        Guid userId,
        string eventType,
        string? detail = null,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            type = eventType,
            detail,
            at = DateTime.UtcNow
        });

        var key = ListKey(userId);
        await _db.ListLeftPushAsync(key, payload);
        await _db.ListTrimAsync(key, 0, MaxEntriesPerUser - 1);
        await _db.KeyExpireAsync(key, ListTtl);
    }
}
