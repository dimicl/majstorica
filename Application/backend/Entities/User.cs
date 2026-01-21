using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using backend.Enums;

namespace backend.Entities;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } 
    
    [BsonElement("email")]
    public string Email { get; set; }
    
    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; }
    
    [BsonElement("role")]
    public Role Role { get; set; }
    
    [BsonElement("isActive")]
    public bool IsActive { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}