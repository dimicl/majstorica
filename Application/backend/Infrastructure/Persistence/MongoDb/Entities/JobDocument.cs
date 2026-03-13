using backend.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class JobDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid AssignedMasterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = default!;
    public DateTime? PreferredDateUtc { get; set; }
    public Money? Budget { get; set; }
    public bool IsEmergency { get; set; }
    public string Status { get; set; } = default!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
