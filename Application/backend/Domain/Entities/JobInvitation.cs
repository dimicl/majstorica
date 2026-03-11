using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Entities;

/*Kako radi u praksi, poziv individualnom majstoru:
var invitation = new JobInvitation(
    Guid.NewGuid(),
    jobId,
    clientUserId,
    invitedMasterId: masterId,
    invitedCompanyId: null,
    createdAtUtc: DateTime.UtcNow,
    message: "Da li možete da dođete sutra?");

Poziv firmi:
var invitation = new JobInvitation(
    Guid.NewGuid(),
    jobId,
    clientUserId,
    invitedMasterId: null,
    invitedCompanyId: companyId,
    createdAtUtc: DateTime.UtcNow,
    message: "Potrebna nam je intervencija za poslovni prostor.");

Ako prihvate:
invitation.Accept(DateTime.UtcNow);

if (invitation.InvitedMasterId.HasValue)
    job.AssignToMaster(invitation.InvitedMasterId.Value, DateTime.UtcNow);
else
    job.AssignToCompany(invitation.InvitedCompanyId!.Value, DateTime.UtcNow);


Razlika između JobApplication i JobInvitation
JobApplication
-izvršilac se prijavljuje na marketplace posao

JobInvitation
-klijent direktno poziva izvršioca*/

public class JobInvitation
{
    private JobInvitation()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public JobInvitation(
        Guid id,
        Guid jobId,
        Guid clientUserId,
        Guid? invitedMasterId,
        Guid? invitedCompanyId,
        DateTime createdAtUtc,
        string? message = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Job invitation id cannot be empty.");

        if (jobId == Guid.Empty)
            throw new DomainException("Job id cannot be empty.");

        if (clientUserId == Guid.Empty)
            throw new DomainException("Client user id cannot be empty.");

        ValidateInvitee(invitedMasterId, invitedCompanyId);

        Id = id;
        JobId = jobId;
        ClientUserId = clientUserId;
        InvitedMasterId = invitedMasterId;
        InvitedCompanyId = invitedCompanyId;
        Message = NormalizeOptionalText(message, 2000);

        Status = InvitationStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid JobId { get; private set; }

    public Guid ClientUserId { get; private set; }

    public Guid? InvitedMasterId { get; private set; }

    public Guid? InvitedCompanyId { get; private set; }

    public string? Message { get; private set; }

    public InvitationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? RespondedAtUtc { get; private set; }

    public bool IsPending() => Status == InvitationStatus.Pending;
    public bool IsAccepted() => Status == InvitationStatus.Accepted;
    public bool IsRejected() => Status == InvitationStatus.Rejected;
    public bool IsCancelled() => Status == InvitationStatus.Cancelled;

    public bool IsForMaster() => InvitedMasterId.HasValue;
    public bool IsForCompany() => InvitedCompanyId.HasValue;

    public void UpdateMessage(string? message)
    {
        EnsurePending();

        Message = NormalizeOptionalText(message, 2000);
        Touch();
    }

    public void Accept(DateTime respondedAtUtc)
    {
        EnsurePending();

        Status = InvitationStatus.Accepted;
        RespondedAtUtc = respondedAtUtc;

        Touch();
    }

    public void Reject(DateTime respondedAtUtc)
    {
        EnsurePending();

        Status = InvitationStatus.Rejected;
        RespondedAtUtc = respondedAtUtc;

        Touch();
    }

    public void Cancel(DateTime cancelledAtUtc)
    {
        EnsurePending();

        Status = InvitationStatus.Cancelled;
        RespondedAtUtc = cancelledAtUtc;

        Touch();
    }

    private void EnsurePending()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException("Only pending invitation can be modified.");
    }

    private static void ValidateInvitee(Guid? invitedMasterId, Guid? invitedCompanyId)
    {
        var hasMaster = invitedMasterId.HasValue && invitedMasterId.Value != Guid.Empty;
        var hasCompany = invitedCompanyId.HasValue && invitedCompanyId.Value != Guid.Empty;

        if (!hasMaster && !hasCompany)
            throw new DomainException("Invitation must have either invited master id or invited company id.");

        if (hasMaster && hasCompany)
            throw new DomainException("Invitation cannot target both master and company.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new DomainException($"Text cannot be longer than {maxLength} characters.");

        return normalized;
    }
}