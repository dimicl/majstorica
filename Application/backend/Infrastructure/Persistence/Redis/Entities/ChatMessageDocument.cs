using Redis.OM.Modeling;
using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence.Redis.Entities;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "chatmessage" })]
public class ChatMessageDocument
{
    [RedisIdField]
    public Guid Id { get; set; }

    [Indexed]
    public Guid ConversationId { get; set; }

    [Indexed]
    public Guid SenderId { get; set; }

    public MessageType Type { get; set; }

    public string Content { get; set; } = default!;

    [Indexed(Sortable = true)]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsSystemMessage { get; set; }
}
