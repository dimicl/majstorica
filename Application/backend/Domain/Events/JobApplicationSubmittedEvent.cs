namespace backend.Domain.Events;

public class JobApplicationSubmittedEvent : DomainEvent
{
    public JobApplicationSubmittedEvent(
        Guid applicationId,
        Guid jobId,
        Guid? applicantMasterId,
        Guid? applicantCompanyId,
        DateTime submittedAtUtc)
    {
        if (applicationId == Guid.Empty)
            throw new ArgumentException("Application id cannot be empty.", nameof(applicationId));

        if (jobId == Guid.Empty)
            throw new ArgumentException("Job id cannot be empty.", nameof(jobId));

        var hasMaster = applicantMasterId.HasValue && applicantMasterId.Value != Guid.Empty;
        var hasCompany = applicantCompanyId.HasValue && applicantCompanyId.Value != Guid.Empty;

        if (!hasMaster && !hasCompany)
            throw new ArgumentException("Application must have either applicant master id or applicant company id.");

        if (hasMaster && hasCompany)
            throw new ArgumentException("Application cannot belong to both master and company.");

        ApplicationId = applicationId;
        JobId = jobId;
        ApplicantMasterId = applicantMasterId;
        ApplicantCompanyId = applicantCompanyId;
        SubmittedAtUtc = submittedAtUtc;
    }

    public Guid ApplicationId { get; }

    public Guid JobId { get; }

    public Guid? ApplicantMasterId { get; }

    public Guid? ApplicantCompanyId { get; }

    public DateTime SubmittedAtUtc { get; }
}