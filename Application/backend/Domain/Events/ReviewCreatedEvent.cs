using backend.Domain.Enums;
using backend.Domain.ValueObjects;

namespace backend.Domain.Events;

public class ReviewCreatedEvent : DomainEvent
{
    public ReviewCreatedEvent(
        Guid reviewId,
        Guid jobId,
        Guid reviewerUserId,
        ReviewTargetType targetType,
        Guid? targetMasterId,
        Guid? targetCompanyId,
        Rating rating,
        DateTime createdAtUtc)
    {
        if (reviewId == Guid.Empty)
            throw new ArgumentException("Review id cannot be empty.", nameof(reviewId));

        if (jobId == Guid.Empty)
            throw new ArgumentException("Job id cannot be empty.", nameof(jobId));

        if (reviewerUserId == Guid.Empty)
            throw new ArgumentException("Reviewer user id cannot be empty.", nameof(reviewerUserId));

        if (rating is null)
            throw new ArgumentNullException(nameof(rating));

        var hasMaster = targetMasterId.HasValue && targetMasterId.Value != Guid.Empty;
        var hasCompany = targetCompanyId.HasValue && targetCompanyId.Value != Guid.Empty;

        if (targetType == ReviewTargetType.Master)
        {
            if (!hasMaster || hasCompany)
                throw new ArgumentException("Review target is invalid for master review.");
        }
        else if (targetType == ReviewTargetType.Company)
        {
            if (!hasCompany || hasMaster)
                throw new ArgumentException("Review target is invalid for company review.");
        }
        else
        {
            throw new ArgumentException("Invalid review target type.", nameof(targetType));
        }

        ReviewId = reviewId;
        JobId = jobId;
        ReviewerUserId = reviewerUserId;
        TargetType = targetType;
        TargetMasterId = targetMasterId;
        TargetCompanyId = targetCompanyId;
        Rating = rating;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid ReviewId { get; }

    public Guid JobId { get; }

    public Guid ReviewerUserId { get; }

    public ReviewTargetType TargetType { get; }

    public Guid? TargetMasterId { get; }

    public Guid? TargetCompanyId { get; }

    public Rating Rating { get; }

    public DateTime CreatedAtUtc { get; }
}