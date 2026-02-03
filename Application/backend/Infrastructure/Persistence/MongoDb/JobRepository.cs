using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb;

namespace backend.Infrastructure.Persistence.MongoDb;

public class JobRepository : IJobRepository
{
    private readonly MongoJobRepository _mongo;
    private readonly IJobGraphRepository _graph;

    public JobRepository(MongoJobRepository mongo, IJobGraphRepository graph)
    {
        _mongo = mongo;
        _graph = graph;
    }

    public async Task<Job?> GetById(Guid id) => await _mongo.GetById(id);

    public async Task Save(Job job)
    {
        await _mongo.Save(job);
        await _graph.MergeJobNode(job.Id);
    }

    public Task InviteMasters(Guid jobId, IEnumerable<Guid> masterIds) =>
        _graph.InviteMasters(jobId, masterIds);

    public Task<List<Guid>> GetInvitedMasters(Guid jobId) =>
        _graph.GetInvitedMasters(jobId);

    public Task AcceptMaster(Guid jobId, Guid masterId) =>
        _graph.AcceptMaster(jobId, masterId);
}
