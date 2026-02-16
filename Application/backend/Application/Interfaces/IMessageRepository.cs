using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IMessageRepository
{
    Task Save(ChatMessage message);

    Task<List<ChatMessage>> GetByConversationId(Guid conversationId);

    Task<ChatMessage?> GetLastByConversationId(Guid conversationId);
}
