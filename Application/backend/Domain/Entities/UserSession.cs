using Redis.OM.Modeling;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "usersession" })]
public class UserSession
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

    public string ConnectionId { get; set; } = default!;

    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}
