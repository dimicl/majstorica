using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Events;

namespace backend.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IRedisLockService _lockService;
    private readonly IMessagePublisher _messagePublisher;

    public JobService(
        IJobRepository jobRepository,
        IRedisLockService lockService,
        IMessagePublisher messagePublisher)
    {
        _jobRepository = jobRepository;
        _lockService = lockService;
        _messagePublisher = messagePublisher;
    }

    public async Task<Guid> CreateJob(Guid clientId, string description)
    {
        var job = new Job(clientId, description);
        if (job == null)
            throw new Exception("Posao nije pronadjen.");

        await _jobRepository.Save(job);

        return job.Id;
    }


    public async Task AssignMaster(Guid jobId, Guid masterId)
    {
        var job = await _jobRepository.GetById(jobId);
        if (job == null)
            throw new Exception("Posao nije pronadjen.");

        job.AssignMaster(masterId);

        await _jobRepository.Save(job);

        await PublishEvents(job);
    }

    public async Task ChangeDescription(Guid jobId, string description, Guid userId)
    {
        await _lockService.EnsureWriteAccess(jobId, userId);

        var job = await _jobRepository.GetById(jobId);
        if (job == null)
            throw new Exception("Posao nije pronadjen.");

        job.ChangeDescription(description);

        await _jobRepository.Save(job);

        await PublishEvents(job);
    }

    public async Task StartJob(Guid jobId)
    {
        var job = await _jobRepository.GetById(jobId);
        if (job == null)
            throw new Exception("Posao nije pronadjen.");

        job.Start();

        await _jobRepository.Save(job);

        await PublishEvents(job);
    }

    public async Task CompleteJob(Guid jobId)
    {
        var job = await _jobRepository.GetById(jobId);
        if (job == null)
            throw new Exception("Posao nije pronadjen.");

        job.Complete();

        await _jobRepository.Save(job);

        await PublishEvents(job);
    }

    private async Task PublishEvents(Job job)
    {
        foreach (var domainEvent in job.DomainEvents)
        {
            await _messagePublisher.Publish(domainEvent);
        }

        job.ClearEvents();
    }
}
