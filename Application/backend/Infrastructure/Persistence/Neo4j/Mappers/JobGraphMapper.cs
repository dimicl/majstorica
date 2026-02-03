using backend.Infrastructure.Persistence.Neo4j.Entities;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j.Mappers;

/// <summary>
/// Mapiranje Job čvora i relacija u Neo4j: entitet ↔ parametri za Cypher, IRecord → Guid.
/// </summary>
public static class JobGraphMapper
{
    public static JobNode ToNode(Guid jobId)
    {
        return new JobNode { Id = jobId };
    }

    /// <summary>
    /// Parametri za MERGE (j:Job { id }).
    /// </summary>
    public static object ToMergeParameters(JobNode node)
    {
        return new { id = node.Id.ToString() };
    }

    /// <summary>
    /// Parametri za InviteMasters (UNWIND masters, MERGE User, MERGE (j)-[:INVITED]->(m)).
    /// </summary>
    public static object ToInvitedMastersParameters(Guid jobId, IEnumerable<Guid> masterIds)
    {
        return new
        {
            jobId = jobId.ToString(),
            masters = masterIds.Select(id => id.ToString()).ToList()
        };
    }

    /// <summary>
    /// Parametri za AcceptMaster (DELETE r, MERGE (j)-[:ACCEPTED_BY]->(m)).
    /// </summary>
    public static object ToAcceptMasterParameters(Guid jobId, Guid masterId)
    {
        return new
        {
            jobId = jobId.ToString(),
            masterId = masterId.ToString()
        };
    }

    /// <summary>
    /// Iz Neo4j record-a (RETURN m.id AS id) izvlači Guid majstora.
    /// </summary>
    public static Guid FromRecordToMasterId(IRecord record)
    {
        return Guid.Parse(record["id"].As<string>());
    }
}
