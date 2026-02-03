using backend.Domain.Entities;
using backend.Infrastructure.Persistence.Redis.Entities;

namespace backend.Infrastructure.Persistence.Redis.Mappers;
public static class ChatMessageMapper
{
    public static ChatMessageDocument ToEntity(ChatMessage message)
    {
        return new ChatMessageDocument
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            JobId = message.JobId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt
        };
    }

    public static ChatMessage ToDomain(ChatMessageDocument doc)
    {
        return ChatMessage.FromPersistence(
            doc.Id,
            doc.ConversationId,
            doc.JobId,
            doc.SenderId,
            doc.Content,
            doc.SentAt);
    }
}
