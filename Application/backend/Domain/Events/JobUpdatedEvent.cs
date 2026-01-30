namespace backend.Domain.Events;


//ovo nam kaze da je neki podatak na job-u primenjen, kreira se u job-u
public class JobUpdatedEvent : IDomainEvent
{
    public Guid JobId { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public JobUpdatedEvent(Guid jobId)
    {
        JobId = jobId;
    }
}
