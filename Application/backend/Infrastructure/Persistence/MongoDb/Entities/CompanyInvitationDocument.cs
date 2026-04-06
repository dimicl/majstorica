using backend.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class CompanyInvitationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid CompanyId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid MasterUserId { get; set; }

    public CompanyInvitationStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
