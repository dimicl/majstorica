using backend.Application.Interfaces;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisAuthServerSessionStore : IAuthServerSessionStore
{
    private readonly IDatabase _db;
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(7);

    private static string Key(Guid userId) => $"auth:usersession:{userId:D}";

    public RedisAuthServerSessionStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public Task TouchServerSessionAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.StringSetAsync(Key(userId), DateTime.UtcNow.ToString("o"), Ttl);
}
