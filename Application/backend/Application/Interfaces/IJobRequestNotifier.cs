namespace backend.Application.Interfaces;

public interface IJobRequestNotifier
{
    Task NotifyNewRequest(
        Guid masterId,
        Guid jobId,
        Guid conversationId,
        string jobTitle,
        string description,
        DateTime date,
        string clientName,
        Guid clientId,
        decimal? price,
        bool isEmergency);
}
