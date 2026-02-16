using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class MessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = default!;

    [BsonRepresentation(BsonType.String)]
    public Guid ConversationId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid JobId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid SenderId { get; set; }

    public string Content { get; set; } = default!;

    public DateTime SentAt { get; set; }
}
