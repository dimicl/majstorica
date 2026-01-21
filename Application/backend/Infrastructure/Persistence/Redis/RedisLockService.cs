using backend.Application.Interfaces;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisLockService : IRedisLockService
{
    private readonly IDatabase _database;

    public RedisLockService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task EnsureWriteAccess(Guid documentId, Guid userId)
    {
        var lockKey = $"lock:job:{documentId}";
        var currentOwner = await _database.StringGetAsync(lockKey);

        if (currentOwner.IsNull)
        {
            // niko nema lock → uzmi ga
            var acquired = await _database.StringSetAsync(
                lockKey,
                userId.ToString(),
                TimeSpan.FromSeconds(30),
                When.NotExists);

            if (!acquired)
                throw new Exception("Dokument je zaključan od strane drugog korisnika.");
        }
        else if (currentOwner != userId.ToString())
        {
            throw new Exception("Nemate pravo izmene. Dokument je read-only.");
        }

        // ako je owner isti → OK
    }
}
