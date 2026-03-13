namespace backend.Api.DTOs.Conversation;

public class ChatMessageResponse
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid? JobId { get; init; }
    public Guid SenderId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime SentAtUtc { get; init; }
    public bool IsSystemMessage { get; init; }
}
