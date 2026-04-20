using backend.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class ConversationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid? JobId { get; set; }
    public Guid ClientId { get; set; }
    public Guid MasterId { get; set; }

    public ConversationType Type { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CompanyId { get; set; }
    public bool IsClosed { get; set; }
}
