using backend.Domain.Enums;
using backend.Infrastructure.Persistence.Neo4j.Entities;

namespace backend.Infrastructure.Persistence.Neo4j.Mappers;

/// <summary>
/// Mapiranje User čvora u Neo4j grafu: entitet ↔ parametri za Cypher.
/// </summary>
public static class UserGraphMapper
{
    public static UserNode ToNode(Guid userId, UserRole role)
    {
        return new UserNode { Id = userId, Role = role };
    }

    /// <summary>
    /// Parametri za MERGE (u:User { id }) SET u.role.
    /// </summary>
    public static object ToMergeParameters(UserNode node)
    {
        return new
        {
            id = node.Id.ToString(),
            role = node.Role.ToString()
        };
    }
}
