namespace backend.Domain.Entities;

public class Client
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    public DateTime? UpdatedAt { get; internal set; }

    protected Client() { }

    public Client(Guid userId, string? phone = null, string? deliveryAddress = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }


    public static Client Rehydrate(
        Guid id,
        Guid userId,
        DateTime createdAt,
        DateTime? updatedAt
    )
    {
        return new Client
        {
            Id = id,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
