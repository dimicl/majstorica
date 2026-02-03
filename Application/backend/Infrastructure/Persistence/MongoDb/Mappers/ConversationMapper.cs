using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class ConversationMapper
{
    public static ConversationDocument ToDocument(ChatConversation conversation)
    {
        return new ConversationDocument
        {
            Id = conversation.Id,
            JobId = conversation.JobId,
            ClientId = conversation.ClientId,
            MasterId = conversation.MasterId,
            IsActive = conversation.IsActive
        };
    }

    public static ChatConversation ToDomain(ConversationDocument doc)
    {
        return ChatConversation.Rehydrate(
            doc.Id,
            doc.JobId,
            doc.ClientId,
            doc.MasterId,
            doc.IsActive);
    }
}
