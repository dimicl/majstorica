using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class ConversationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid ClientId { get; set; }
    public Guid MasterId { get; set; }
    public bool IsActive { get; set; }
}
