using Redis.OM.Modeling;
using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence.Redis.Entities;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "usersession" })]
public class UserSessionDocument
{
    [RedisIdField]
    public string Id { get; set; } = default!;
    
    [Indexed]
    public Guid UserId { get; set; }

    [Indexed]
    public UserRole Role { get; set; }

    [Indexed]
    public Guid? CurrentJobId { get; set; }

    [Indexed]
    public Guid? CurrentConversationId { get; set; }

    [Indexed]
    public string ConnectionId { get; set; } = default!;

    /// <summary>Poslednja aktivnost; koristi se u chatu za prikaz "poslednje aktivnosti".</summary>
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}
