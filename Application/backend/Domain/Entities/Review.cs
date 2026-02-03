namespace backend.Domain.Entities;
public class Review
{
    public Guid Id { get; internal set; }

    public Guid JobId { get; internal set; }
    public Guid ClientId { get; internal set; }
    public Guid MasterId { get; internal set; }

    public int Rating { get; internal set; }

    public string? Comment { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    protected Review() { }

    public static Review Rehydrate(Guid id, Guid jobId, Guid clientId, Guid masterId, int rating, string? comment, DateTime createdAt)
    {
        return new Review
        {
            Id = id,
            JobId = jobId,
            ClientId = clientId,
            MasterId = masterId,
            Rating = rating,
            Comment = comment,
            CreatedAt = createdAt
        };
    }

    public Review(Guid jobId, Guid clientId, Guid masterId, int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Ocena mora biti između 1 i 5.");

        Id = Guid.NewGuid();
        JobId = jobId;
        ClientId = clientId;
        MasterId = masterId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}
