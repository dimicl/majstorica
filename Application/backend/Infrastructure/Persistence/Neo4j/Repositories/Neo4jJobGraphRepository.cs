using backend.Application.Interfaces;
using backend.Infrastructure.Persistence.Neo4j.Mappers;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j;

public class Neo4jJobGraphRepository : IJobGraphRepository
{
    private readonly IDriver _driver;

    public Neo4jJobGraphRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task MergeJobNode(Guid jobId)
    {
        var node = JobGraphMapper.ToNode(jobId);
        var parameters = JobGraphMapper.ToMergeParameters(node);

        var query = @"
            MERGE (j:Job { id: $id })
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx => tx.RunAsync(query, parameters));
    }

    public async Task InviteMasters(Guid jobId, IEnumerable<Guid> masterIds)
    {
        var parameters = JobGraphMapper.ToInvitedMastersParameters(jobId, masterIds);

        var query = @"
            MATCH (j:Job { id: $jobId })
            UNWIND $masters AS masterId
            MERGE (m:User { id: masterId })
            MERGE (j)-[:INVITED]->(m)
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx => tx.RunAsync(query, parameters));
    }

    public async Task<List<Guid>> GetInvitedMasters(Guid jobId)
    {
        var query = @"
            MATCH (j:Job { id: $jobId })-[:INVITED]->(m:User)
            RETURN m.id AS id
        ";

        await using var session = _driver.AsyncSession();

        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { jobId = jobId.ToString() });
            var result = new List<Guid>();

            while (await cursor.FetchAsync())
            {
                result.Add(JobGraphMapper.FromRecordToMasterId(cursor.Current));
            }

            return result;
        });
    }

    public async Task AcceptMaster(Guid jobId, Guid masterId)
    {
        var parameters = JobGraphMapper.ToAcceptMasterParameters(jobId, masterId);

        var query = @"
            MATCH (j:Job { id: $jobId })-[r:INVITED]->(m:User)
            WHERE m.id = $masterId
            DELETE r
            MERGE (j)-[:ACCEPTED_BY]->(m)
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx => tx.RunAsync(query, parameters));
    }

    public async Task RecordHired(Guid clientId, Guid masterId, Guid jobId, DateTime completedAt, int? rating)
    {
        var parameters = JobGraphMapper.ToRecordHiredParameters(clientId, masterId, jobId, completedAt, rating);

        var query = @"
            MERGE (c:Client { id: $clientId })
            MERGE (m:Master { id: $masterId })
            CREATE (c)-[:HIRED { jobId: $jobId, completedAt: $completedAt, rating: $rating }]->(m)
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx => tx.RunAsync(query, parameters));
    }
}
