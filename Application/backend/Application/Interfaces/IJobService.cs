namespace backend.Application.Interfaces;

public interface IJobService
{
    Task<Guid> CreateJob(
        Guid clientId,
        string description,
        decimal? price,
        bool isEmergency);

    Task SendRequests(Guid jobId, List<Guid> masterIds);

    Task AcceptJob(Guid jobId, Guid masterId);
    
    Task StartJob(Guid jobId);
    Task CompleteJob(Guid jobId);

    Task ChangeDescription(Guid jobId, Guid userId, string description);
    Task ChangePrice(Guid jobId, Guid userId, decimal? price);
}
