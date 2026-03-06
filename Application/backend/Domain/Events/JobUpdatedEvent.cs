namespace backend.Domain.Events;

//ovo nam kaze da je neki podatak na job-u primenjen, implementra IDomainEvent
public class JobUpdatedEvent : IDomainEvent
{
    public Guid JobId { get; }
    public DateTime OccurredAt { get; }

    public JobUpdatedEvent(Guid jobId)
    {
        JobId = jobId;
        OccurredAt = DateTime.UtcNow;
    }

    //Za deserijalizaciju iz RabbitMQ (System.Text.Json)
    public JobUpdatedEvent(Guid jobId, DateTime occurredAt)
    {
        JobId = jobId;
        OccurredAt = occurredAt;
    }
}
