using backend.Application.Interfaces;
using backend.Shared.Exceptions;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisLockService : IRedisLockService
{
    private readonly IDatabase _db;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromSeconds(30);

    public RedisLockService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string LockKey(Guid jobId) => $"lock:job:{jobId}";
    private static string QueueKey(Guid jobId) => $"queue:job:{jobId}";

    public async Task<Guid?> GetOwner(Guid jobId)
    {
        var v = await _db.StringGetAsync(LockKey(jobId));
        return v.IsNullOrEmpty ? null : Guid.Parse(v!);
    }

    public async Task EnsureWriteAccess(Guid jobId, Guid userId)
    {
        var owner = await GetOwner(jobId);

        if (owner == null)
        {
            var acquired = await _db.StringSetAsync(
                LockKey(jobId),
                userId.ToString(),
                DefaultTtl,
                When.NotExists);

            if (!acquired)
                throw new ConflictException("Dokument je zaključan.");
            return;
        }

        if (owner == userId)
        {
            await _db.KeyExpireAsync(LockKey(jobId), DefaultTtl);
            return;
        }

        await Enqueue(jobId, userId);
        throw new ForbiddenException("Nemate write pristup. Dokument je read-only.");
    }

    public async Task<Guid?> ReleaseWriteAccess(Guid jobId, Guid userId)
    {
        var owner = await GetOwner(jobId);
        if (owner != userId)
            return null;

        var next = await Dequeue(jobId);

        if (next == null)
        {
            await _db.KeyDeleteAsync(LockKey(jobId));
            return null;
        }

        await _db.StringSetAsync(
            LockKey(jobId),
            next.ToString(),
            DefaultTtl);

        return next;
    }

    private async Task Enqueue(Guid jobId, Guid userId)
    {
        var list = await _db.ListRangeAsync(QueueKey(jobId), 0, -1);
        if (list.Any(x => x == userId.ToString())) return;

        await _db.ListRightPushAsync(QueueKey(jobId), userId.ToString());
    }

    private async Task<Guid?> Dequeue(Guid jobId)
    {
        var v = await _db.ListLeftPopAsync(QueueKey(jobId));
        return v.IsNullOrEmpty ? null : Guid.Parse(v!);
    }
}
