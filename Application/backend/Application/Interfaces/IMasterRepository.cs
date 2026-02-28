using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IMasterRepository
{
    Task Save(Master master);
    Task<Master?> GetById(Guid id);
    Task<Master?> GetByUserId(Guid userId);
    Task<List<Master>> GetByUserIds(IEnumerable<Guid> userIds);
}
