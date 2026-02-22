using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class MessageMapper
{
    public static MessageDocument ToDocument(ChatMessage message)
    {
        return new MessageDocument
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            JobId = message.JobId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt,
            IsSystemMessage = message.IsSystemMessage
        };
    }

    public static ChatMessage ToDomain(MessageDocument doc)
    {
        return ChatMessage.FromPersistence(
            doc.Id,
            doc.ConversationId,
            doc.JobId,
            doc.SenderId,
            doc.Content,
            doc.SentAt,
            doc.IsSystemMessage
        );
    }
}
