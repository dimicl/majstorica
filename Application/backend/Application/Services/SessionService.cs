using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repository;

    public SessionService(ISessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserSession> CreateOrUpdateSession(
        Guid userId,
        UserRole role,
        string connectionId)
    {
        var session = await _repository.GetByUserId(userId)
            ?? new UserSession(userId.ToString(), userId, role, connectionId);

        session.ConnectionId = connectionId;
        session.LastSeen = DateTime.UtcNow;

        await _repository.Upsert(session);
        return session;
    }

    public async Task MarkUserInJob(Guid userId, Guid jobId)
    {
        var session = await _repository.GetByUserId(userId);
        if (session == null) return;

        session.CurrentJobId = jobId;
        await _repository.Upsert(session);
    }

    public async Task MarkUserInConversation(Guid userId, Guid conversationId)
    {
        var session = await _repository.GetByUserId(userId);
        if (session == null) return;

        session.CurrentConversationId = conversationId;
        await _repository.Upsert(session);
    }

    public async Task HandleDisconnect(string connectionId)
    {
        var session = await _repository.GetByConnectionId(connectionId);
        if (session == null) return;

        await _repository.Remove(session.Id);
    }

    public async Task<bool> IsUserOnlineAsync(Guid userId)
    {
        var session = await _repository.GetByUserId(userId);
        return session != null;
    }
}
