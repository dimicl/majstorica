using backend.Domain.Enums;
using backend.Domain.Exceptions;
using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

public class Review
{
    private Review()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Review(
        Guid id,
        Guid jobId,
        Guid reviewerUserId,
        ReviewTargetType targetType,
        Rating rating,
        DateTime createdAtUtc,
        Guid? targetMasterId = null,
        Guid? targetCompanyId = null,
        string? comment = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Review id cannot be empty.");

        if (jobId == Guid.Empty)
            throw new DomainException("Job id cannot be empty.");

        if (reviewerUserId == Guid.Empty)
            throw new DomainException("Reviewer user id cannot be empty.");

        ValidateTarget(targetType, targetMasterId, targetCompanyId);

        Id = id;
        JobId = jobId;
        ReviewerUserId = reviewerUserId;
        TargetType = targetType;
        TargetMasterId = targetMasterId;
        TargetCompanyId = targetCompanyId;
        Rating = rating ?? throw new DomainException("Rating is required.");
        Comment = NormalizeOptionalText(comment, 2000);

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        IsEdited = false;
    }

    public Guid Id { get; private set; }

    public Guid JobId { get; private set; }

    public Guid ReviewerUserId { get; private set; }

    public ReviewTargetType TargetType { get; private set; }

    public Guid? TargetMasterId { get; private set; }

    public Guid? TargetCompanyId { get; private set; }

    public Rating Rating { get; private set; } = null!;

    public string? Comment { get; private set; }

    public bool IsEdited { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? EditedAtUtc { get; private set; }

    public bool IsForMaster() => TargetType == ReviewTargetType.Master;

    public bool IsForCompany() => TargetType == ReviewTargetType.Company;

    public void Update(Rating rating, string? comment, DateTime editedAtUtc)
    {
        Rating = rating ?? throw new DomainException("Rating is required.");
        Comment = NormalizeOptionalText(comment, 2000);
        IsEdited = true;
        EditedAtUtc = editedAtUtc;

        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void ValidateTarget(
        ReviewTargetType targetType,
        Guid? targetMasterId,
        Guid? targetCompanyId)
    {
        var hasMaster = targetMasterId.HasValue && targetMasterId.Value != Guid.Empty;
        var hasCompany = targetCompanyId.HasValue && targetCompanyId.Value != Guid.Empty;

        if (targetType == ReviewTargetType.Master)
        {
            if (!hasMaster)
                throw new DomainException("Review for master must have target master id.");

            if (hasCompany)
                throw new DomainException("Review for master cannot have target company id.");

            return;
        }

        if (targetType == ReviewTargetType.Company)
        {
            if (!hasCompany)
                throw new DomainException("Review for company must have target company id.");

            if (hasMaster)
                throw new DomainException("Review for company cannot have target master id.");

            return;
        }

        throw new DomainException("Invalid review target type.");
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