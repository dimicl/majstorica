namespace backend.Application.Interfaces;

public interface IRedisLockService
{
    Task EnsureWriteAccess(Guid jobId, Guid userId);

    Task<Guid?> ReleaseWriteAccess(Guid jobId, Guid userId);

    Task<Guid?> GetOwner(Guid jobId);
}
