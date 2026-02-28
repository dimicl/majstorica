namespace backend.Api.DTOs.Jobs;

public class JobListItemResponse
{
    public Guid JobId { get; init; }
    public Guid ConversationId { get; init; }

    public string JobTitle { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string? MasterName { get; init; }

    public DateTime Date { get; init; }
    public Guid ClientId { get; init; }
    public decimal? Price { get; init; }
    public bool IsEmergency { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
