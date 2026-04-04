using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence.MongoDb;

public class JobRepository : IJobRepository
{
    private readonly IMongoJobRepository _mongo;
    private readonly IJobGraphRepository _graph;

    public JobRepository(IMongoJobRepository mongo, IJobGraphRepository graph)
    {
        _mongo = mongo;
        _graph = graph;
    }

    public async Task<Job?> GetById(Guid id) => await _mongo.GetById(id);

    public Task<List<Job>> GetByMasterIdAndStatuses(Guid masterId, IEnumerable<JobStatus> statuses) =>
        _mongo.GetByMasterIdAndStatuses(masterId, statuses);

    public Task<List<Job>> GetByClientId(Guid clientId) =>
        _mongo.GetByClientId(clientId);

    public Task<List<Job>> GetAllPaginated(int page, int pageSize) =>
        _mongo.GetAllPaginated(page, pageSize);

    public async Task Save(Job job)
    {
        await _mongo.Save(job);
        await _graph.MergeJobNode(job.Id);
    }

    public Task MergeJobNode(Guid jobId) =>
        _graph.MergeJobNode(jobId);

    public Task InviteMasters(Guid jobId, IEnumerable<Guid> masterIds) =>
        _graph.InviteMasters(jobId, masterIds);

    public Task<List<Guid>> GetInvitedMasters(Guid jobId) =>
        _graph.GetInvitedMasters(jobId);

    public Task AcceptMaster(Guid jobId, Guid masterId) =>
        _graph.AcceptMaster(jobId, masterId);

    public Task RecordHired(Guid clientId, Guid masterId, Guid jobId, DateTime completedAt, int? rating) =>
        _graph.RecordHired(clientId, masterId, jobId, completedAt, rating);
}
