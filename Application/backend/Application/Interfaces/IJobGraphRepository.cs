namespace backend.Application.Interfaces;

public interface IJobGraphRepository
{
    Task MergeJobNode(Guid jobId);
    Task InviteMasters(Guid jobId, IEnumerable<Guid> masterIds);
    Task<List<Guid>> GetInvitedMasters(Guid jobId);
    Task AcceptMaster(Guid jobId, Guid masterId);
}
