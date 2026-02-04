using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class ClientDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
