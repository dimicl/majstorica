using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence.Neo4j.Entities;

public class UserNode
{
    public Guid Id { get; set; }
    public UserRole Role { get; set; }
}
