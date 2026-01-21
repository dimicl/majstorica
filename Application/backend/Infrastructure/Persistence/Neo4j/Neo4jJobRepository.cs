using backend.Application.Interfaces;
using backend.Domain.Entities;
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
            SET j.description = $description,
                j.status = $status
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx =>
            tx.RunAsync(query, new
            {
                id = job.Id.ToString(),
                description = job.Description,
                status = job.Status.ToString()
            })
        );
    }

    public async Task<Job?> GetById(Guid id)
    {
        var query = @"
            MATCH (j:Job { id: $id })
            RETURN j.id AS id, j.description AS description, j.status AS status
        ";

        await using var session = _driver.AsyncSession();

        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { id = id.ToString() });

            if (await cursor.FetchAsync() == false)
                return null;

            return cursor.Current;
        });

        if (result == null)
            return null;

        var description = result["description"].As<string>();
        var status = result["status"].As<string>();

        return Job.Rehydrate(id, description, status);
    }

}
