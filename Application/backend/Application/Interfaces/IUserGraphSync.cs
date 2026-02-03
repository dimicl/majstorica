using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IUserGraphSync
{
    Task SyncUserNode(Guid userId, UserRole role);
}
