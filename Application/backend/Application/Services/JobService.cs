using backend.Api.DTOs.Jobs;
using backend.Application.Helpers;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Strategies;

namespace backend.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IRedisLockService _lockService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IJobRequestNotifier _jobRequestNotifier;

    public JobService(
        IJobRepository jobRepository,
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IMessageRepository messageRepository,
        IRedisLockService lockService,
        IMessagePublisher messagePublisher,
        IJobRequestNotifier jobRequestNotifier)
    {
        _jobRepository = jobRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _lockService = lockService;
        _messagePublisher = messagePublisher;
        _jobRequestNotifier = jobRequestNotifier;
    }


    public async Task<Guid> CreateJob(
        Guid clientId,
        string title,
        string description,
        DateTime? scheduledDate,
        decimal? price,
        bool isEmergency)
    {
        var job = new Job(clientId, title, description, scheduledDate, isEmergency, DateTime.UtcNow, price);

        IBookingStrategy strategy =
            isEmergency
                ? new EmergencyBookingStrategy()
                : new NormalBookingStrategy();

        strategy.Apply(job);

        await _jobRepository.Save(job);
        await PublishEvents(job);

        return job.Id;
    }

    public async Task SendRequests(Guid jobId, List<Guid> masterIds)
    {
        var job = await GetJob(jobId);

        job.SendRequests();

        var client = await _userRepository.GetById(job.ClientId);
        var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

        foreach (var masterId in masterIds)
        {
            var existing = await _conversationRepository.GetByClientAndMaster(job.ClientId, masterId);
            ChatConversation conversation;

            if (existing != null)
            {
                existing.Reopen();
                existing.AssignJob(job.Id);
                await _conversationRepository.Save(existing);
                conversation = existing;

                var jobRequestMessage = new ChatMessage(
                    conversation.Id,
                    job.Id,
                    job.ClientId,
                    $"{clientName} ti je poslao zahtev za posao.",
                    isSystemMessage: true);
                await _messageRepository.Save(jobRequestMessage);
                await _conversationRepository.IncrementUnreadAsync(conversation.Id, masterId);
            }
            else
            {
                conversation = new ChatConversation(
                    jobId,
                    job.ClientId,
                    masterId);
                await _conversationRepository.Save(conversation);

                var jobRequestMessage = new ChatMessage(
                    conversation.Id,
                    job.Id,
                    job.ClientId,
                    $"{clientName} ti je poslao zahtev za posao.",
                    isSystemMessage: true);
                await _messageRepository.Save(jobRequestMessage);
                await _conversationRepository.IncrementUnreadAsync(conversation.Id, masterId);
            }

            await _jobRequestNotifier.NotifyNewRequest(
                masterId,
                job.Id,
                conversation.Id,
                job.Title,
                job.Description,
                job.ScheduledDate ?? job.CreatedAt,
                clientName,
                job.ClientId,
                job.Price,
                job.IsEmergency);
        }

        await SaveAndPublish(job);
    }

    public Task<bool> HasClientSentRequestToMaster(Guid clientId, Guid masterId) =>
        _conversationRepository.ExistsByClientAndMaster(clientId, masterId);

    public async Task<List<JobRequestListItemResponse>> GetPendingRequestsForMaster(Guid masterId)
    {
        var conversations = await _conversationRepository.GetByUserId(masterId);
        var result = new List<JobRequestListItemResponse>();

        foreach (var conv in conversations)
        {
            if (conv.MasterId != masterId || !conv.IsActive || conv.JobId == Guid.Empty)
                continue;

            var job = await _jobRepository.GetById(conv.JobId);
            if (job == null || job.Status != JobStatus.Pending)
                continue;

            var client = await _userRepository.GetById(conv.ClientId);
            var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

            result.Add(new JobRequestListItemResponse
            {
                JobId = job.Id,
                ConversationId = conv.Id,
                JobTitle = job.Title,
                Description = job.Description,
                ClientName = clientName,
                ClientId = job.ClientId,
                Date = job.ScheduledDate ?? job.CreatedAt,
                Price = job.Price,
                IsEmergency = job.IsEmergency,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt ?? job.CreatedAt
            });
        }

        return result;
    }

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
            else
            {
                var master = await _userRepository.GetById(masterId);
                var masterName = UserDisplayNameHelper.GetDisplayName(master, "Majstor");
                var systemMessage = new ChatMessage(
                    conversation.Id,
                    jobId,
                    masterId,
                    $"{masterName} je prihvatio zahtev za posao.",
                    isSystemMessage: true);
                await _messageRepository.Save(systemMessage);
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
