using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class MessageMapper
{
    public static MessageDocument ToDocument(Message message)
    {
        return new MessageDocument
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            Type = message.Type,
            Content = message.Content,
            SentAtUtc = message.SentAtUtc,
        };
    }

    public static Message ToDomain(MessageDocument doc)
    {
        return new Message(
            doc.Id,
            doc.ConversationId,
            doc.SenderUserId,
            doc.Type,
            doc.Content,
            doc.SentAtUtc
        );
    }
}
