using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetById(Guid id);
    Task Save(Job job);

    Task InviteMasters(Guid jobId, IEnumerable<Guid> masterIds);
    Task<List<Guid>> GetInvitedMasters(Guid jobId);
    Task AcceptMaster(Guid jobId, Guid masterId);
}
