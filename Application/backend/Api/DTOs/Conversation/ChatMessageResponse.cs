namespace backend.Api.DTOs.Conversation;

public class ChatMessageResponse
{
    public string Id { get; init; } = string.Empty;
    public Guid ConversationId { get; init; }
    public Guid JobId { get; init; }
    public Guid SenderId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public bool IsSystemMessage { get; init; }
}
