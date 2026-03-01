using backend.Infrastructure.Persistence.Neo4j.Entities;

namespace backend.Infrastructure.Persistence.Neo4j.Mappers;

/// <summary>
/// Mapiranje Zone čvora u Neo4j: entitet ↔ parametri za Cypher.
/// </summary>
public static class ZoneGraphMapper
{
    public static object ToMergeParameters(ZoneNode node)
    {
        return new { id = node.Id, name = node.Name };
    }
}
