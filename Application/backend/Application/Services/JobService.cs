using backend.Api.DTOs.Jobs;
using backend.Application.Helpers;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Strategies;
using backend.Domain.ValueObjects;
using backend.Shared.Exceptions;

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
        var now = DateTime.UtcNow;

        var address = new Address(
            street: "Nepoznata adresa",
            city: "Nepoznat grad");

        Money? budget = price.HasValue
            ? new Money(price.Value, "RSD")
            : null;

        var job = new Job(
            id: Guid.NewGuid(),
            clientUserId: clientId,
            title: title,
            description: description,
            serviceCategory: "Ostalo",
            serviceAddress: address,
            requestType: JobRequestType.Marketplace,
            executorType: ExecutorType.Any,
            createdAtUtc: now,
            isEmergency: isEmergency,
            preferredDateUtc: scheduledDate,
            preferredTimeNote: null,
            budget: budget);

        await _jobRepository.Save(job);
        await PublishEvents(job);

        return job.Id;
    }

    public async Task SendRequests(Guid jobId, List<Guid> masterIds)
    {
        var job = await GetJob(jobId);

        var client = await _userRepository.GetById(job.ClientUserId);
        var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

        foreach (var masterId in masterIds)
        {
            var existing = await _conversationRepository.GetByClientAndMaster(job.ClientUserId, masterId);
            Conversation conversation;

            if (existing != null)
            {
                existing.Reopen();
                await _conversationRepository.Save(existing);
                conversation = existing;

                var jobRequestMessage = new Message(
                    conversation.Id,
                    job.Id,
                    job.ClientUserId,
                    MessageType.System,
                    $"{clientName} ti je poslao zahtev za posao.",
                    DateTime.UtcNow);
                await _messageRepository.Save(jobRequestMessage);
                await _conversationRepository.IncrementUnreadAsync(conversation.Id, masterId);
            }
            else
            {
                conversation = new Conversation(
                    Guid.NewGuid(),
                    job.ClientUserId,
                    ConversationType.JobRelated,
                    DateTime.UtcNow,
                    masterId,
                    null,
                    job.Id);
                await _conversationRepository.Save(conversation);

                var jobRequestMessage = new Message(
                    conversation.Id,
                    job.Id,
                    job.ClientUserId,
                    MessageType.System,
                    $"{clientName} ti je poslao zahtev za posao.",
                    DateTime.UtcNow);
                await _messageRepository.Save(jobRequestMessage);
                await _conversationRepository.IncrementUnreadAsync(conversation.Id, masterId);
            }

            await _jobRequestNotifier.NotifyNewRequest(
                masterId,
                job.Id,
                conversation.Id,
                job.Title,
                job.Description,
                job.PreferredDateUtc ?? job.CreatedAtUtc,
                clientName,
                job.ClientUserId,
                job.Budget?.Amount,
                job.IsEmergency);
        }

        await _jobRepository.InviteMasters(jobId, masterIds);
        await SaveAndPublish(job);
    }

    /// <summary>Da li klijent ima posao/zahtev ka tom majstoru (samo ako posao još postoji u bazi).</summary>
    public async Task<bool> HasClientSentRequestToMaster(Guid clientId, Guid masterId)
    {
        var jobs = await _jobRepository.GetByClientId(clientId);
        foreach (var job in jobs)
        {
            if (job.AssignedMasterId == masterId)
                return true;
            if (job.Status == JobStatus.InProgress)
            {
                var convs = await _conversationRepository.GetByJobId(job.Id);
                if (convs.Any(c => c.MasterUserId == masterId))
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
            if (conv.MasterUserId != masterId || !conv.IsClosed || !conv.JobId.HasValue)
                continue;

            var job = await _jobRepository.GetById(conv.JobId!.Value);
            if (job == null || job.Status != JobStatus.InProgress)
                continue;

            var client = await _userRepository.GetById(conv.ClientUserId);
            var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

            result.Add(new JobListItemResponse
            {
                JobId = job.Id,
                ConversationId = conv.Id,
                JobTitle = job.Title,
                Description = job.Description,
                ClientName = clientName,
                MasterName = null,
                Date = job.CreatedAtUtc,
                ClientId = job.ClientUserId,
                Price = job.Budget?.Amount,
                IsEmergency = job.IsEmergency,
                Status = JobStatus.InProgress.ToString(),
                CreatedAt = job.CreatedAtUtc,
                UpdatedAt = job.UpdatedAtUtc
            });
        }

        return result;
    }

    private async Task<List<JobListItemResponse>> GetJobsForMaster(Guid masterId)
    {
        var jobsById = new Dictionary<Guid, Job>();

        var byMasterAndStatus = await _jobRepository.GetByMasterIdAndStatuses(
            masterId,
            new[] { JobStatus.Assigned, JobStatus.InProgress, JobStatus.Completed });
        foreach (var j in byMasterAndStatus)
            jobsById[j.Id] = j;

        var conversations = await _conversationRepository.GetByUserId(masterId);
        foreach (var conv in conversations)
        {
            if (conv.MasterUserId != masterId || !conv.JobId.HasValue) continue;
            if (jobsById.ContainsKey(conv.JobId.Value)) continue;

            var job = await _jobRepository.GetById(conv.JobId.Value);
            if (job == null) continue;
            if (job.Status != JobStatus.Assigned && job.Status != JobStatus.InProgress && job.Status != JobStatus.Completed)
                continue;
            jobsById[job.Id] = job;
        }

        var result = new List<JobListItemResponse>();
        foreach (var job in jobsById.Values.OrderByDescending(j => j.UpdatedAtUtc))
        {
            var convs = await _conversationRepository.GetByJobId(job.Id);
                var conv = convs.FirstOrDefault(c => c.MasterUserId == masterId);
            var conversationId = conv?.Id ?? Guid.Empty;

            var client = await _userRepository.GetById(job.ClientUserId);
            var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

            result.Add(new JobListItemResponse
            {
                JobId = job.Id,
                ConversationId = conversationId,
                JobTitle = job.Title,
                Description = job.Description,
                ClientName = clientName,
                MasterName = null,
                Date = job.CreatedAtUtc,
                ClientId = job.ClientUserId,
                Price = job.Budget?.Amount,
                IsEmergency = job.IsEmergency,
                Status = job.Status.ToString(),
                CreatedAt = job.CreatedAtUtc,
                UpdatedAt = job.UpdatedAtUtc
            });
        }

        return result;
    }

    private async Task<List<JobListItemResponse>> GetJobsForClient(Guid clientId)
    {
        var jobs = await _jobRepository.GetByClientId(clientId);
        var result = new List<JobListItemResponse>();

        foreach (var job in jobs.OrderByDescending(j => j.UpdatedAtUtc))
        {
            var client = await _userRepository.GetById(job.ClientUserId);
            var clientName = UserDisplayNameHelper.GetDisplayName(client, "Klijent");

            string? masterName = null;
            var conversationId = Guid.Empty;
            if (job.AssignedMasterId.HasValue)
            {
                var master = await _userRepository.GetById(job.AssignedMasterId.Value);
                masterName = UserDisplayNameHelper.GetDisplayName(master, "Majstor");
                var convs = await _conversationRepository.GetByJobId(job.Id);
                var conv = convs.FirstOrDefault(c => c.MasterUserId == job.AssignedMasterId.Value);
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
                Date = job.CreatedAtUtc,
                ClientId = job.ClientUserId,
                Price = job.Budget?.Amount,
                IsEmergency = job.IsEmergency,
                Status = job.Status.ToString(),
                CreatedAt = job.CreatedAtUtc,
                UpdatedAt = job.UpdatedAtUtc
            });
        }

        return result;
    }

    public async Task AcceptJob(Guid jobId, Guid masterId)
    {
        var job = await GetJob(jobId);

        job.AssignToMaster(masterId, DateTime.UtcNow);

        var conversations = await _conversationRepository.GetByJobId(jobId);

        foreach (var conversation in conversations)
        {
            if (conversation.MasterUserId != masterId)
            {
                conversation.Close();
            }
            else
            {
                var master = await _userRepository.GetById(masterId);
                var masterName = UserDisplayNameHelper.GetDisplayName(master, "Majstor");
                var systemMessage = new Message(
                    Guid.NewGuid(),
                    conversation.Id,
                    masterId,
                    MessageType.System,
                    $"{masterName} je prihvatio zahtev za posao.",
                    DateTime.UtcNow);
                await _messageRepository.Save(systemMessage);
            }
        }

        await _conversationRepository.SaveMany(conversations);

        await _jobRepository.AcceptMaster(jobId, masterId);
        await SaveAndPublish(job);
    }


    public async Task StartJob(Guid jobId)
    {
        var job = await GetJob(jobId);

        job.Start(DateTime.UtcNow);

        await SaveAndPublish(job);
    }

    public async Task CompleteJob(Guid jobId)
    {
        var job = await GetJob(jobId);

        job.Complete(DateTime.UtcNow);

        await SaveAndPublish(job);

        if (job.AssignedMasterId.HasValue)
            await _jobRepository.RecordHired(job.ClientUserId, job.AssignedMasterId.Value, job.Id, DateTime.UtcNow, rating: null);
    }


    public async Task ChangeDescription(Guid jobId, Guid userId, string description)
    {
        await _lockService.EnsureWriteAccess(jobId, userId);

        var job = await GetJob(jobId);

        job.UpdateBasicInfo(
            job.Title,
            description,
            job.ServiceCategory,
            job.ServiceAddress,
            job.Budget);

        await SaveAndPublish(job);
    }

    public async Task ChangePrice(Guid jobId, Guid userId, decimal? price)
    {
        await _lockService.EnsureWriteAccess(jobId, userId);

        var job = await GetJob(jobId);

        Money? budget = price.HasValue
            ? new Money(price.Value, job.Budget?.Currency ?? "RSD")
            : null;

        job.SetBudget(budget);

        await SaveAndPublish(job);
    }


    private async Task<Job> GetJob(Guid jobId)
    {
        var job = await _jobRepository.GetById(jobId);
        if (job == null)
            throw new NotFoundException("Posao nije pronađen.");

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

        job.ClearDomainEvents();
    }
}
