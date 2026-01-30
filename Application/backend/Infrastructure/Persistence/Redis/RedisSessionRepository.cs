using backend.Application.Interfaces;
using backend.Domain.Entities;
using Redis.OM;
using Redis.OM.Searching;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisSessionRepository : ISessionRepository
{
    private readonly RedisCollection<UserSession> _sessions;

    public RedisSessionRepository(RedisConnectionProvider provider)
    {
        _sessions = (RedisCollection<UserSession>)provider.RedisCollection<UserSession>();
    }

    public async Task Upsert(UserSession session)
    {
        await _sessions.InsertAsync(session);
    }

    public Task<UserSession?> GetByUserId(Guid userId)
    {
        return Task.FromResult(
            _sessions.FirstOrDefault(s => s.UserId == userId)
        );
    }

    public Task<UserSession?> GetByConnectionId(string connectionId)
    {
        return Task.FromResult(
            _sessions.FirstOrDefault(s => s.ConnectionId == connectionId)
        );
    }

    public Task<List<UserSession>> GetByJobId(Guid jobId)
    {
        return Task.FromResult(
            _sessions.Where(s => s.CurrentJobId == jobId).ToList()
        );
    }

    public Task<List<UserSession>> GetAll()
    {
        return Task.FromResult(_sessions.ToList());
    }

    public async Task Remove(string sessionId)
    {
        var session = await _sessions.FindByIdAsync(sessionId);
        if (session != null)
            _sessions.Delete(session);
    }
}
