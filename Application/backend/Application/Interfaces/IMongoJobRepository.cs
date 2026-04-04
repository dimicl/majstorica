using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IMongoJobRepository
{
    Task Save(Job job);
    Task<Job?> GetById(Guid id);
    Task<List<Job>> GetByMasterIdAndStatuses(Guid masterId, IEnumerable<JobStatus> statuses);
    Task<List<Job>> GetByClientId(Guid clientId);
    Task<List<Job>> GetAllPaginated(int page, int pageSize);
}
