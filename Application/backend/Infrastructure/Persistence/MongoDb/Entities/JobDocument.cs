using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class JobDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid? MasterId { get; set; }
    public string Description { get; set; } = default!;
    public decimal? Price { get; set; }
    public bool IsEmergency { get; set; }
    public string Status { get; set; } = default!;
}
