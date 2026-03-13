using backend.Domain.Entities;
using backend.Infrastructure.Persistence.Redis.Entities;

namespace backend.Infrastructure.Persistence.Redis.Mappers;

public static class UserSessionMapper
{
    public static UserSessionDocument ToEntity(UserSession session)
    {
        return new UserSessionDocument
        {
            Id = session.Id,
            UserId = session.UserId,
            Role = session.Role,
            CurrentJobId = session.CurrentJobId,
            CurrentConversationId = session.CurrentConversationId,
            ConnectionId = session.ConnectionId,
            LastSeen = session.LastSeen
        };
    }

    public static UserSession ToDomain(UserSessionDocument doc)
    {
        return new UserSession(
            doc.Id,
            doc.UserId,
            doc.Role,
            doc.CurrentJobId,
            doc.CurrentConversationId,
            doc.ConnectionId,
            doc.LastSeen);
    }
}
