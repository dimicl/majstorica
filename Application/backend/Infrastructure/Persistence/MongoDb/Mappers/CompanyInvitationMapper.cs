using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class CompanyInvitationMapper
{
    public static CompanyInvitationDocument ToDocument(CompanyInvitation invitation) =>
        new()
        {
            Id = invitation.Id,
            CompanyId = invitation.CompanyId,
            MasterUserId = invitation.MasterUserId,
            Status = invitation.Status,
            CreatedAtUtc = invitation.CreatedAtUtc
        };

    public static CompanyInvitation ToDomain(CompanyInvitationDocument doc) =>
        new(doc.Id, doc.CompanyId, doc.MasterUserId, doc.Status, doc.CreatedAtUtc);
}
