using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.Redis.Entities;
using backend.Infrastructure.Persistence.Redis.Mappers;
using Microsoft.Extensions.Configuration;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisSessionRepository : ISessionRepository
{
    private const string KeyPrefix = "usersession:";
    private static string LastSeenKey(Guid userId) => $"lastseen:{userId}";
    private static readonly TimeSpan LastSeenTtl = TimeSpan.FromDays(30);

    private readonly RedisCollection<UserSessionDocument> _sessions;
    private readonly IDatabase _db;
    private readonly TimeSpan _sessionTtl;

    public RedisSessionRepository(
        RedisConnectionProvider provider,
        IConnectionMultiplexer redis,
        IConfiguration configuration)
    {
        _sessions = (RedisCollection<UserSessionDocument>)provider.RedisCollection<UserSessionDocument>();
        _db = redis.GetDatabase();
        var minutes = configuration.GetValue("Session:TtlMinutes", 30);
        _sessionTtl = TimeSpan.FromMinutes(minutes);
    }

    public async Task Upsert(UserSession session)
    {
        var doc = UserSessionMapper.ToEntity(session);
        await _sessions.InsertAsync(doc);
        await _db.KeyExpireAsync(KeyPrefix + doc.Id, _sessionTtl);
    }

    public Task<UserSession?> GetByUserId(Guid userId)
    {
        var doc = _sessions.FirstOrDefault(s => s.UserId == userId);
        return Task.FromResult(doc == null ? null : UserSessionMapper.ToDomain(doc));
    }

    public Task<UserSession?> GetByConnectionId(string connectionId)
    {
        var doc = _sessions.FirstOrDefault(s => s.ConnectionId == connectionId);
        return Task.FromResult(doc == null ? null : UserSessionMapper.ToDomain(doc));
    }

    public Task<List<UserSession>> GetAll()
    {
        var docs = _sessions.ToList();
        return Task.FromResult(docs.Select(UserSessionMapper.ToDomain).ToList());
    }

    public async Task Remove(string sessionId)
    {
        var doc = await _sessions.FindByIdAsync(sessionId);
        if (doc != null)
            _sessions.Delete(doc);
    }

    public Task SaveLastSeenAsync(Guid userId, DateTime lastSeen)
    {
        return _db.StringSetAsync(LastSeenKey(userId), lastSeen.ToString("o"), LastSeenTtl);
    }

    public async Task<DateTime?> GetLastSeenFromStoreAsync(Guid userId)
    {
        var value = await _db.StringGetAsync(LastSeenKey(userId));
        if (value.IsNullOrEmpty) return null;
        return DateTime.TryParse(value.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt
            : null;
    }
}
  