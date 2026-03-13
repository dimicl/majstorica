using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class ReviewDocument
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid ReviewerUserId { get; set; }

    public ReviewTargetType TargetType { get; set; }
    public Guid? TargetMasterId { get; set; }
    public Guid? TargetCompanyId { get; set; }

    public decimal Rating { get; set; }
    public string? Comment { get; set; }

    public bool IsEdited { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? EditedAtUtc { get; set; }
}