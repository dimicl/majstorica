namespace backend.Domain.Entities;

public class Client
{
    public Guid UserId { get; internal set; }

    public string? Phone { get; internal set; }

    public string? DeliveryAddress { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    public DateTime? UpdatedAt { get; internal set; }

    protected Client() { }

    public Client(Guid userId, string? phone = null, string? deliveryAddress = null)
    {
        UserId = userId;
        Phone = phone;
        DeliveryAddress = deliveryAddress;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContact(string? phone, string? deliveryAddress)
    {
        Phone = phone;
        DeliveryAddress = deliveryAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Client Rehydrate(
        Guid userId,
        string? phone,
        string? deliveryAddress,
        DateTime createdAt,
        DateTime? updatedAt
    )
    {
        return new Client
        {
            UserId = userId,
            Phone = phone,
            DeliveryAddress = deliveryAddress,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
