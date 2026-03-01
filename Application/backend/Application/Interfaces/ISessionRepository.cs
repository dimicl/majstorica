using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface ISessionRepository
{
    Task Upsert(UserSession session);

    Task<UserSession?> GetByUserId(Guid userId);

    Task<UserSession?> GetByConnectionId(string connectionId);

    Task<List<UserSession>> GetAll();

    Task Remove(string sessionId);
}
