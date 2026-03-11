namespace backend.Domain.Events;

public class JobPublishedEvent : DomainEvent
{
    public JobPublishedEvent(Guid jobId, Guid clientUserId, DateTime publishedAtUtc)
    {
        if (jobId == Guid.Empty)
            throw new ArgumentException("Job id cannot be empty.", nameof(jobId));

        if (clientUserId == Guid.Empty)
            throw new ArgumentException("Client user id cannot be empty.", nameof(clientUserId));

        JobId = jobId;
        ClientUserId = clientUserId;
        PublishedAtUtc = publishedAtUtc;
    }

    public Guid JobId { get; }

    public Guid ClientUserId { get; }

    public DateTime PublishedAtUtc { get; }
}