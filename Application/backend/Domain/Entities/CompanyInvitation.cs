using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Entities;

public class CompanyInvitation
{
    private CompanyInvitation()
    {
    }

    public CompanyInvitation(
        Guid id,
        Guid companyId,
        Guid masterUserId,
        CompanyInvitationStatus status,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException("Invitation id cannot be empty.");
        if (companyId == Guid.Empty)
            throw new DomainException("Company id cannot be empty.");
        if (masterUserId == Guid.Empty)
            throw new DomainException("Master user id cannot be empty.");

        Id = id;
        CompanyId = companyId;
        MasterUserId = masterUserId;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid MasterUserId { get; private set; }

    public CompanyInvitationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static CompanyInvitation CreatePending(Guid companyId, Guid masterUserId) =>
        new(
            Guid.NewGuid(),
            companyId,
            masterUserId,
            CompanyInvitationStatus.Pending,
            DateTime.UtcNow);

    public void MarkAccepted()
    {
        if (Status != CompanyInvitationStatus.Pending)
            throw new DomainException("Only pending invitations can be accepted.");
        Status = CompanyInvitationStatus.Accepted;
    }

    public void MarkDeclined()
    {
        if (Status != CompanyInvitationStatus.Pending)
            throw new DomainException("Only pending invitations can be declined.");
        Status = CompanyInvitationStatus.Declined;
    }
}
