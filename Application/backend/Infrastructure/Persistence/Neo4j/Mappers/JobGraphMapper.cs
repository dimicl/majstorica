using backend.Infrastructure.Persistence.Neo4j.Entities;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j.Mappers;

public static class JobGraphMapper
{
    public static JobNode ToNode(Guid jobId)
    {
        return new JobNode { Id = jobId };
    }

    public static object ToMergeParameters(JobNode node)
    {
        return new { id = node.Id.ToString() };
    }

    public static object ToInvitedMastersParameters(Guid jobId, IEnumerable<Guid> masterIds)
    {
        return new
        {
            jobId = jobId.ToString(),
            masters = masterIds.Select(id => id.ToString()).ToList()
        };
    }

    public static object ToAcceptMasterParameters(Guid jobId, Guid masterId)
    {
        return new
        {
            jobId = jobId.ToString(),
            masterId = masterId.ToString()
        };
    }
    public static Guid FromRecordToMasterId(IRecord record)
    {
        return Guid.Parse(record["id"].As<string>());
    }

    public static object ToRecordHiredParameters(Guid clientId, Guid masterId, Guid jobId, DateTime completedAt, int? rating)
    {
        return new
        {
            clientId = clientId.ToString(),
            masterId = masterId.ToString(),
            jobId = jobId.ToString(),
            completedAt = completedAt.ToString("O"),
            rating = rating ?? 0
        };
    }
}
