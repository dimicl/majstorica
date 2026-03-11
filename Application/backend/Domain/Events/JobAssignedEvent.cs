namespace backend.Domain.Events;

public class JobAssignedEvent : DomainEvent
{
    public JobAssignedEvent(
        Guid jobId,
        Guid clientUserId,
        Guid? assignedMasterId,
        Guid? assignedCompanyId,
        DateTime assignedAtUtc)
    {
        if (jobId == Guid.Empty)
            throw new ArgumentException("Job id cannot be empty.", nameof(jobId));

        if (clientUserId == Guid.Empty)
            throw new ArgumentException("Client user id cannot be empty.", nameof(clientUserId));

        var hasMaster = assignedMasterId.HasValue && assignedMasterId.Value != Guid.Empty;
        var hasCompany = assignedCompanyId.HasValue && assignedCompanyId.Value != Guid.Empty;

        if (!hasMaster && !hasCompany)
            throw new ArgumentException("Assigned event must contain either assigned master id or assigned company id.");

        if (hasMaster && hasCompany)
            throw new ArgumentException("Assigned event cannot contain both master and company.");

        JobId = jobId;
        ClientUserId = clientUserId;
        AssignedMasterId = assignedMasterId;
        AssignedCompanyId = assignedCompanyId;
        AssignedAtUtc = assignedAtUtc;
    }

    public Guid JobId { get; }

    public Guid ClientUserId { get; }

    public Guid? AssignedMasterId { get; }

    public Guid? AssignedCompanyId { get; }

    public DateTime AssignedAtUtc { get; }
}