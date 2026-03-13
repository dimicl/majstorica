using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IMessageRepository
{
    Task Save(Message message);

    Task<List<Message>> GetByConversationId(Guid conversationId);

    Task<Message?> GetLastByConversationId(Guid conversationId);
}
