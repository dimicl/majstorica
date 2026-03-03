using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetById(Guid id);
    Task Save(Job job);

    Task<List<Job>> GetByMasterIdAndStatuses(Guid masterId, IEnumerable<JobStatus> statuses);
    Task<List<Job>> GetByClientId(Guid clientId);

    Task InviteMasters(Guid jobId, IEnumerable<Guid> masterIds);
    Task<List<Guid>> GetInvitedMasters(Guid jobId);
    Task AcceptMaster(Guid jobId, Guid masterId);
    /// <summary>Neo4j: upis istorije angažovanja (Client)-[:HIRED]->(Master) za preporuke.</summary>
    Task RecordHired(Guid clientId, Guid masterId, Guid jobId, DateTime completedAt, int? rating);
}
