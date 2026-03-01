using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.Redis.Entities;
using backend.Infrastructure.Persistence.Redis.Mappers;
using Redis.OM;
using Redis.OM.Searching;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisSessionRepository : ISessionRepository
{
    private readonly RedisCollection<UserSessionDocument> _sessions;

    public RedisSessionRepository(RedisConnectionProvider provider)
    {
        _sessions = (RedisCollection<UserSessionDocument>)provider.RedisCollection<UserSessionDocument>();
    }

    public async Task Upsert(UserSession session)
    {
        var doc = UserSessionMapper.ToEntity(session);
        await _sessions.InsertAsync(doc);
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
}
  