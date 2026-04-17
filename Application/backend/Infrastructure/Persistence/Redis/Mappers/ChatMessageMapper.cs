using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Persistence.Redis.Entities;

namespace backend.Infrastructure.Persistence.Redis.Mappers;
public static class ChatMessageMapper
{
    public static ChatMessageDocument ToEntity(Message message)
    {
        return new ChatMessageDocument
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderUserId,
            Type = message.Type,
            Content = message.Content,
            SentAt = message.SentAtUtc,
            IsSystemMessage = message.Type == MessageType.System
        };

    }

    public static Message ToDomain(ChatMessageDocument doc)
    {
        return new Message(
            doc.Id,
            doc.ConversationId,
            doc.SenderId,
            doc.Type,
            doc.Content,
            doc.SentAt        );
    }
}
