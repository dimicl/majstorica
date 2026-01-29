using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j;

public class Neo4jJobRepository : IJobRepository
{
    private readonly IDriver _driver;

    public Neo4jJobRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task Save(Job job)
    {
        var query = @"
            MERGE (j:Job { id: $id })
            SET j.clientId = $clientId,
                j.masterId = $masterId,
                j.description = $description,
                j.price = $price,
                j.status = $status
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx =>
            tx.RunAsync(query, new
            {
                id = job.Id.ToString(),
                clientId = job.ClientId.ToString(),
                masterId = job.MasterId?.ToString(),
                description = job.Description,
                price = job.Price,
                status = job.Status.ToString()
            })
        );
    }

    public async Task<Job?> GetById(Guid id)
    {
        var query = @"
            MATCH (j:Job { id: $id })
            RETURN 
                j.id AS id,
                j.clientId AS clientId,
                j.masterId AS masterId,
                j.description AS description,
                j.price AS price,
                j.status AS status
        ";

        await using var session = _driver.AsyncSession();

        var record = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { id = id.ToString() });

            if (!await cursor.FetchAsync())
                return null;

            return cursor.Current;
        });

        if (record == null)
            return null;

        return Job.Rehydrate(
            Guid.Parse(record["id"].As<string>()),
            Guid.Parse(record["clientId"].As<string>()),
            record["masterId"].As<string>() is string m ? Guid.Parse(m) : null,
            record["description"].As<string>(),
            record["price"].As<decimal?>(),
            record["status"].As<string>()
        );
    }

    public async Task InviteMasters(Guid jobId, IEnumerable<Guid> masterIds)
    {
        var query = @"
            MATCH (j:Job { id: $jobId })
            UNWIND $masters AS masterId
            MERGE (m:User { id: masterId })
            MERGE (j)-[:INVITED]->(m)
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx =>
            tx.RunAsync(query, new
            {
                jobId = jobId.ToString(),
                masters = masterIds.Select(id => id.ToString()).ToList()
            })
        );
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
                result.Add(Guid.Parse(cursor.Current["id"].As<string>()));
            }

            return result;
        });
    }

    public async Task AcceptMaster(Guid jobId, Guid masterId)
    {
        var query = @"
            MATCH (j:Job { id: $jobId })-[r:INVITED]->(m:User)
            WHERE m.id = $masterId
            DELETE r
            MERGE (j)-[:ACCEPTED_BY]->(m)
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx =>
            tx.RunAsync(query, new
            {
                jobId = jobId.ToString(),
                masterId = masterId.ToString()
            })
        );
    }
}
