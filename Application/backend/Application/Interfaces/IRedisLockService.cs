namespace backend.Application.Interfaces;

public interface IRedisLockService
{
    Task EnsureWriteAccess(Guid documentId, Guid userId);
}
