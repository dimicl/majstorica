using backend.Domain.Enums;
using backend.Domain.Exceptions;
using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

/*Kako radi u praksi, primer prijave individualnog majstora
var application = new JobApplication(
    Guid.NewGuid(),
    jobId,
    applicantMasterId: masterId,
    applicantCompanyId: null,
    createdAtUtc: DateTime.UtcNow,
    message: "Mogu da dođem sutra ujutru.",
    proposedPrice: new Money(50, "EUR"));

Primer prijave firme
var application = new JobApplication(
    Guid.NewGuid(),
    jobId,
    applicantMasterId: null,
    applicantCompanyId: companyId,
    createdAtUtc: DateTime.UtcNow,
    message: "Naša ekipa može da izađe na teren.",
    proposedPrice: new Money(120, "EUR"));

Kada klijent prihvati prijavu, u servisu bi logika bila:
application.Accept(DateTime.UtcNow);

if (application.ApplicantMasterId.HasValue)
    job.AssignToMaster(application.ApplicantMasterId.Value, DateTime.UtcNow);
else
    job.AssignToCompany(application.ApplicantCompanyId!.Value, DateTime.UtcNow);*/

public class JobApplication
{
    private JobApplication()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public JobApplication(
        Guid id,
        Guid jobId,
        Guid? applicantMasterId,
        Guid? applicantCompanyId,
        DateTime createdAtUtc,
        string? message = null,
        Money? proposedPrice = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Job application id cannot be empty.");

        if (jobId == Guid.Empty)
            throw new DomainException("Job id cannot be empty.");

        ValidateApplicant(applicantMasterId, applicantCompanyId);

        Id = id;
        JobId = jobId;
        ApplicantMasterId = applicantMasterId;
        ApplicantCompanyId = applicantCompanyId;

        Message = NormalizeOptionalText(message, 2000);
        ProposedPrice = proposedPrice;
        Status = ApplicationStatus.Pending;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid JobId { get; private set; }

    public Guid? ApplicantMasterId { get; private set; }

    public Guid? ApplicantCompanyId { get; private set; }

    public string? Message { get; private set; }

    public Money? ProposedPrice { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? RespondedAtUtc { get; private set; }

    public bool IsPending() => Status == ApplicationStatus.Pending;
    public bool IsAccepted() => Status == ApplicationStatus.Accepted;
    public bool IsRejected() => Status == ApplicationStatus.Rejected;
    public bool IsWithdrawn() => Status == ApplicationStatus.Withdrawn;

    public bool IsFromMaster() => ApplicantMasterId.HasValue;
    public bool IsFromCompany() => ApplicantCompanyId.HasValue;

    public void UpdateMessage(string? message)
    {
        EnsurePending();

        Message = NormalizeOptionalText(message, 2000);
        Touch();
    }

    public void UpdateProposedPrice(Money? proposedPrice)
    {
        EnsurePending();

        ProposedPrice = proposedPrice;
        Touch();
    }

    public void Accept(DateTime respondedAtUtc)
    {
        EnsurePending();

        Status = ApplicationStatus.Accepted;
        RespondedAtUtc = respondedAtUtc;

        Touch();
    }

    public void Reject(DateTime respondedAtUtc)
    {
        EnsurePending();

        Status = ApplicationStatus.Rejected;
        RespondedAtUtc = respondedAtUtc;

        Touch();
    }

    public void Withdraw(DateTime respondedAtUtc)
    {
        EnsurePending();

        Status = ApplicationStatus.Withdrawn;
        RespondedAtUtc = respondedAtUtc;

        Touch();
    }

    private void EnsurePending()
    {
        if (Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending application can be modified.");
    }

    private static void ValidateApplicant(Guid? applicantMasterId, Guid? applicantCompanyId)
    {
        var hasMaster = applicantMasterId.HasValue && applicantMasterId.Value != Guid.Empty;
        var hasCompany = applicantCompanyId.HasValue && applicantCompanyId.Value != Guid.Empty;

        if (!hasMaster && !hasCompany)
            throw new DomainException("Application must have either applicant master id or applicant company id.");

        if (hasMaster && hasCompany)
            throw new DomainException("Application cannot belong to both master and company.");
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