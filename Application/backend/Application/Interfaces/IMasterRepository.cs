using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IMasterRepository
{
    Task Save(Guid userId, MasterProfile masterProfile);

    Task<MasterProfile?> GetByUserId(Guid userId);

    Task<IReadOnlyDictionary<Guid, MasterProfile?>> GetByUserIds(IEnumerable<Guid> userIds);
}
