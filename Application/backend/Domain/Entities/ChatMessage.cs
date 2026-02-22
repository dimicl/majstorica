namespace backend.Domain.Entities;

public class ChatMessage
{
    public string Id { get; internal set; } = Guid.NewGuid().ToString();
    public Guid ConversationId { get; internal set; }
    public Guid JobId { get; internal set; }
    public Guid SenderId { get; internal set; }
    public string Content { get; internal set; } = default!;
    public DateTime SentAt { get; internal set; } = DateTime.UtcNow;
    public bool IsSystemMessage { get; internal set; }

    protected ChatMessage() { }

    public ChatMessage(Guid conversationId, Guid jobId, Guid senderId, string content, bool isSystemMessage = false)
    {
        Id = Guid.NewGuid().ToString();
        ConversationId = conversationId;
        JobId = jobId;
        SenderId = senderId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        SentAt = DateTime.UtcNow;
        IsSystemMessage = isSystemMessage;
    }

    public static ChatMessage FromPersistence(string id, Guid conversationId, Guid jobId, Guid senderId, string content, DateTime sentAt, bool isSystemMessage = false)
    {
        return new ChatMessage
        {
            Id = id,
            ConversationId = conversationId,
            JobId = jobId,
            SenderId = senderId,
            Content = content,
            SentAt = sentAt,
            IsSystemMessage = isSystemMessage
        };
    }
}
