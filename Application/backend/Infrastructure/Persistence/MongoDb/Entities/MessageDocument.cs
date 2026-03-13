using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class MessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid ConversationId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid SenderUserId { get; set; }

    public MessageType Type { get; set; }

    public string Content { get; set; } = default!;

    public DateTime SentAtUtc { get; set; }

}
