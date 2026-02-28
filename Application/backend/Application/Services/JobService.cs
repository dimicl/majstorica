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

    /// <summary>Da li klijent ima posao/zahtev ka tom majstoru (samo ako posao još postoji u bazi).</summary>
    public async Task<bool> HasClientSentRequestToMaster(Guid clientId, Guid masterId)
    {
        var jobs = await _jobRepository.GetByClientId(clientId);
        foreach (var job in jobs)
        {
            if (job.MasterId == masterId)
                return true;
            if (job.Status == JobStatus.Pending)
            {
                var convs = await _conversationRepository.GetByJobId(job.Id);
                if (convs.Any(c => c.MasterId == masterId))
                    return true;
            }
        }
        return false;
    }

    public async Task<List<JobListItemResponse>> GetJobsForUser(Guid userId, UserRole role)
    {
        if (role == UserRole.Master)
        {
            var pending = await GetPendingRequestsForMaster(userId);
            var assigned = await GetJobsForMaster(userId);
            return pending.Concat(assigned).OrderByDescending(j => j.UpdatedAt).ToList();
        }

        return await GetJobsForClient(userId);
    }

    private async Task<List<JobListItemResponse>> GetPendingRequestsForMaster(Guid masterId)
    {
        var conversations = await _conversationRepository.GetByUserId(masterId);
        var result = new List<JobListItemResponse>();

        foreach (var conv in conversations)
        {
            if (conv.MasterId != masterId || !conv.IsActive || conv.JobId == Guid.Empty)
                continue;

            var job = await _jobRepository.GetById(conv.JobId);
            if (job == null || job.Status != JobStatus.Pending)
                continue;

            var client = await _userRepository.GetById(conv.ClientId);
            var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

            result.Add(new JobListItemResponse
            {
                JobId = job.Id,
                ConversationId = conv.Id,
                JobTitle = job.Title,
                Description = job.Description,
                ClientName = clientName,
                MasterName = null,
                Date = job.ScheduledDate ?? job.CreatedAt,
                ClientId = job.ClientId,
                Price = job.Price,
                IsEmergency = job.IsEmergency,
                Status = JobStatus.Pending.ToString(),
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt ?? job.CreatedAt
            });
        }

        return result;
    }

    private async Task<List<JobListItemResponse>> GetJobsForMaster(Guid masterId)
    {
        var jobsById = new Dictionary<Guid, Job>();

        var byMasterAndStatus = await _jobRepository.GetByMasterIdAndStatuses(
            masterId,
            new[] { JobStatus.Accepted, JobStatus.InProgress, JobStatus.Completed });
        foreach (var j in byMasterAndStatus)
            jobsById[j.Id] = j;

        var conversations = await _conversationRepository.GetByUserId(masterId);
        foreach (var conv in conversations)
        {
            if (conv.MasterId != masterId || conv.JobId == Guid.Empty) continue;
            if (jobsById.ContainsKey(conv.JobId)) continue;

            var job = await _jobRepository.GetById(conv.JobId);
            if (job == null) continue;
            if (job.Status != JobStatus.Accepted && job.Status != JobStatus.InProgress && job.Status != JobStatus.Completed)
                continue;
            jobsById[job.Id] = job;
        }

        var result = new List<JobListItemResponse>();
        foreach (var job in jobsById.Values.OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt))
        {
            var convs = await _conversationRepository.GetByJobId(job.Id);
            var conv = convs.FirstOrDefault(c => c.MasterId == masterId);
            var conversationId = conv?.Id ?? Guid.Empty;

            var client = await _userRepository.GetById(job.ClientId);
            var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

            result.Add(new JobListItemResponse
            {
                JobId = job.Id,
                ConversationId = conversationId,
                JobTitle = job.Title,
                Description = job.Description,
                ClientName = clientName,
                MasterName = null,
                Date = job.ScheduledDate ?? job.CreatedAt,
                ClientId = job.ClientId,
                Price = job.Price,
                IsEmergency = job.IsEmergency,
                Status = job.Status.ToString(),
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt ?? job.CreatedAt
            });
        }

        return result;
    }

    private async Task<List<JobListItemResponse>> GetJobsForClient(Guid clientId)
    {
        var jobs = await _jobRepository.GetByClientId(clientId);
        var result = new List<JobListItemResponse>();

        foreach (var job in jobs.OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt))
        {
            var client = await _userRepository.GetById(job.ClientId);
            var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

            string? masterName = null;
            var conversationId = Guid.Empty;
            if (job.MasterId.HasValue)
            {
                var master = await _userRepository.GetById(job.MasterId.Value);
                masterName = UserDisplayNameHelper.GetDisplayName(master, "Majstor");
                var convs = await _conversationRepository.GetByJobId(job.Id);
                var conv = convs.FirstOrDefault(c => c.MasterId == job.MasterId.Value);
                conversationId = conv?.Id ?? Guid.Empty;
            }

            result.Add(new JobListItemResponse
            {
                JobId = job.Id,
                ConversationId = conversationId,
                JobTitle = job.Title,
                Description = job.Description,
                ClientName = clientName,
                MasterName = masterName,
                Date = job.ScheduledDate ?? job.CreatedAt,
                ClientId = job.ClientId,
                Price = job.Price,
                IsEmergency = job.IsEmergency,
                Status = job.Status.ToString(),
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
