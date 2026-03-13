using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class ConversationMapper
{
    public static ConversationDocument ToDocument(Conversation conversation)
    {
        return new ConversationDocument
        {
            Id = conversation.Id,
            JobId = conversation.JobId,
            ClientId = conversation.ClientUserId,
            MasterId = conversation.MasterUserId,
            CompanyId = conversation.CompanyId,
            Type = conversation.Type,
            CreatedAtUtc = conversation.CreatedAtUtc
        };
    }

    public static Conversation ToDomain(ConversationDocument doc)
    {
        return new Conversation(
            doc.Id,
            doc.ClientId,
            doc.Type,
            doc.CreatedAtUtc,
            doc.MasterId,
            doc.CompanyId,
            doc.JobId
            );
        
    }
}
