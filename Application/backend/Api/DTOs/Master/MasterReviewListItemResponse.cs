namespace backend.Api.DTOs.Master;

public sealed class MasterReviewListItemResponse
{
    public Guid Id { get; init; }

    public Guid JobId { get; init; }

    public decimal Rating { get; init; }

    public string? Comment { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public string ReviewerName { get; init; } = string.Empty;

    public string? ReviewerUsername { get; init; }
}
