using backend.Domain.ValueObjects;

namespace backend.Domain.Events;

public class JobCompletedEvent : DomainEvent
{
    public JobCompletedEvent(
        Guid jobId,
        Guid clientUserId,
        Guid? assignedMasterId,
        Guid? assignedCompanyId,
        DateTime completedAtUtc,
        Money? finalPrice = null)
    {
        if (jobId == Guid.Empty)
            throw new ArgumentException("Job id cannot be empty.", nameof(jobId));

        if (clientUserId == Guid.Empty)
            throw new ArgumentException("Client user id cannot be empty.", nameof(clientUserId));

        JobId = jobId;
        ClientUserId = clientUserId;
        AssignedMasterId = assignedMasterId;
        AssignedCompanyId = assignedCompanyId;
        CompletedAtUtc = completedAtUtc;
        FinalPrice = finalPrice;
    }

    public Guid JobId { get; }

    public Guid ClientUserId { get; }

    public Guid? AssignedMasterId { get; }

    public Guid? AssignedCompanyId { get; }

    public DateTime CompletedAtUtc { get; }

    public Money? FinalPrice { get; }
}