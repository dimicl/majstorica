namespace backend.Domain.Events;


//ovo nam kaze da je neki podatak na job-u primenjen, kreira se u job-u
public class JobUpdatedEvent : IDomainEvent
{
    public Guid JobId { get; }
    public DateTime OccurredAt { get; }

    public JobUpdatedEvent(Guid jobId)
    {
        JobId = jobId;
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>Za deserijalizaciju iz RabbitMQ (System.Text.Json).</summary>
    public JobUpdatedEvent(Guid jobId, DateTime occurredAt)
    {
        JobId = jobId;
        OccurredAt = occurredAt;
    }
}
