using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IChatService
{
    Task<ChatMessage> SendMessage(
        Guid conversationId,
        Guid jobId,
        Guid senderId,
        string content);

    Task<List<ChatMessage>> GetConversationMessages(Guid conversationId);
}
