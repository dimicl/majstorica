using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Strategies;

namespace backend.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IRedisLockService _lockService;
    private readonly IMessagePublisher _messagePublisher;

    public JobService(
        IJobRepository jobRepository,
        IConversationRepository conversationRepository,
        IRedisLockService lockService,
        IMessagePublisher messagePublisher)
    {
        _jobRepository = jobRepository;
        _conversationRepository = conversationRepository;
        _lockService = lockService;
        _messagePublisher = messagePublisher;
    }


    public async Task<Guid> CreateJob(
        Guid clientId,
        string description,
        decimal? price,
        bool isEmergency)
    {
        var job = new Job(clientId, description, price);

        IBookingStrategy strategy =
            isEmergency
                ? new EmergencyBookingStrategy()
                : new NormalBookingStrategy();

        strategy.Apply(job);

        await _jobRepository.Save(job);
        await PublishEvents(job);

        return job.Id;
    }

    // Client bira više majstora -> otvaraju se chat conversation-i

    public async Task SendRequests(Guid jobId, List<Guid> masterIds)
    {
        var job = await GetJob(jobId);

        job.SendRequests();

        foreach (var masterId in masterIds)
        {
            var conversation = new ChatConversation(
                jobId,
                job.ClientId,
                masterId);

            await _conversationRepository.Save(conversation);
        }

        await SaveAndPublish(job);
    }

    // Jedan majstor prihvata -> ostali chatovi se zatvaraju

    public async Task AcceptJob(Guid jobId, Guid masterId)
    {
        var job = await GetJob(jobId);

        job.Accept(masterId);

        var conversations = await _conversationRepository.GetByJobId(jobId);

        foreach (var conversation in conversations)
        {
            if (conversation.MasterId != masterId)
            {
                conversation.Close();
            }
        }

        await _conversationRepository.SaveMany(conversations);

        await SaveAndPublish(job);
    }


    public async Task StartJob(Guid jobId)
    {
        var job = await GetJob(jobId);

        job.Start();

        await SaveAndPublish(job);
    }

    public async Task CompleteJob(Guid jobId)
    {
        var job = await GetJob(jobId);

        job.Complete();

        await SaveAndPublish(job);
    }


    public async Task ChangeDescription(Guid jobId, Guid userId, string description)
    {
        await _lockService.EnsureWriteAccess(jobId, userId);

        var job = await GetJob(jobId);

        job.ChangeDescription(description);

        await SaveAndPublish(job);
    }

    public async Task ChangePrice(Guid jobId, Guid userId, decimal? price)
    {
        await _lockService.EnsureWriteAccess(jobId, userId);

        var job = await GetJob(jobId);

        job.ChangePrice(price);

        await SaveAndPublish(job);
    }


    private async Task<Job> GetJob(Guid jobId)
    {
        var job = await _jobRepository.GetById(jobId);
        if (job == null)
            throw new Exception("Posao nije pronađen.");

        return job;
    }

    private async Task SaveAndPublish(Job job)
    {
        await _jobRepository.Save(job);
        await PublishEvents(job);
    }

    private async Task PublishEvents(Job job)
    {
        foreach (var domainEvent in job.DomainEvents)
            await _messagePublisher.Publish(domainEvent);

        job.ClearEvents();
    }
}
