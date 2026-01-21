namespace backend.Domain.Events;

public class JobUpdatedEvent : IDomainEvent
{
    public Guid JobId { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public JobUpdatedEvent(Guid jobId)
    {
        JobId = jobId;
    }
}
