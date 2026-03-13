using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class ClientDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string? PreferredContactPhone { get; set; }
    public string? Notes { get; set; }
    public int TotalJobsPosted { get; set; }
    public int CompletedJobs { get; set; }
}
