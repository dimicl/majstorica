namespace backend.Application.Interfaces;

public interface IJobService
{
    Task<Guid> CreateJob(Guid clientId, string description);

    Task AssignMaster(Guid jobId, Guid masterId);

    Task ChangeDescription(Guid jobId, string description, Guid userId);

    Task StartJob(Guid jobId);

    Task CompleteJob(Guid jobId);
}
