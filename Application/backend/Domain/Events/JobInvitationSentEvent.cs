namespace backend.Domain.Events;

public class JobInvitationSentEvent : DomainEvent
{
    public JobInvitationSentEvent(
        Guid invitationId,
        Guid jobId,
        Guid clientUserId,
        Guid? invitedMasterId,
        Guid? invitedCompanyId,
        DateTime sentAtUtc)
    {
        if (invitationId == Guid.Empty)
            throw new ArgumentException("Invitation id cannot be empty.", nameof(invitationId));

        if (jobId == Guid.Empty)
            throw new ArgumentException("Job id cannot be empty.", nameof(jobId));

        if (clientUserId == Guid.Empty)
            throw new ArgumentException("Client user id cannot be empty.", nameof(clientUserId));

        var hasMaster = invitedMasterId.HasValue && invitedMasterId.Value != Guid.Empty;
        var hasCompany = invitedCompanyId.HasValue && invitedCompanyId.Value != Guid.Empty;

        if (!hasMaster && !hasCompany)
            throw new ArgumentException("Invitation must target either master or company.");

        if (hasMaster && hasCompany)
            throw new ArgumentException("Invitation cannot target both master and company.");

        InvitationId = invitationId;
        JobId = jobId;
        ClientUserId = clientUserId;
        InvitedMasterId = invitedMasterId;
        InvitedCompanyId = invitedCompanyId;
        SentAtUtc = sentAtUtc;
    }

    public Guid InvitationId { get; }

    public Guid JobId { get; }

    public Guid ClientUserId { get; }

    public Guid? InvitedMasterId { get; }

    public Guid? InvitedCompanyId { get; }

    public DateTime SentAtUtc { get; }
}