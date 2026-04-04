using backend.Api.DTOs.Jobs;
using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IJobService
{
    Task<Guid> CreateJob(
        Guid clientId,
        string title,
        string description,
        DateTime? scheduledDate,
        decimal? price,
        bool isEmergency,
        string? serviceCategory = null);

    Task SendRequests(Guid jobId, List<Guid> masterIds);

    Task<bool> HasClientSentRequestToMaster(Guid clientId, Guid masterId);

    Task<List<JobListItemResponse>> GetJobsForUser(Guid userId, UserRole role);
    Task<List<JobListItemResponse>> GetMarketplaceJobs(int page, int pageSize);

    Task AcceptJob(Guid jobId, Guid masterId);

    Task StartJob(Guid jobId);
    Task CompleteJob(Guid jobId);

    Task ChangeDescription(Guid jobId, Guid userId, string description);
    Task ChangePrice(Guid jobId, Guid userId, decimal? price);
}
