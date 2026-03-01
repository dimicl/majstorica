using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface ISessionService
{
    Task<UserSession> CreateOrUpdateSession(
        Guid userId,
        UserRole role,
        string connectionId);

    Task MarkUserInJob(Guid userId, Guid jobId);

    Task MarkUserInConversation(Guid userId, Guid conversationId);

    Task HandleDisconnect(string connectionId);

    Task<bool> IsUserOnlineAsync(Guid userId);
}
