using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class ClientDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }
    public string? Phone { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
