using Redis.OM.Modeling;

namespace backend.Infrastructure.Persistence.Redis.Entities;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "chatmessage" })]
public class ChatMessageDocument
{
    [RedisIdField]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Indexed]
    public Guid ConversationId { get; set; }

    [Indexed]
    public Guid JobId { get; set; }

    [Indexed]
    public Guid SenderId { get; set; }

    public string Content { get; set; } = default!;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
