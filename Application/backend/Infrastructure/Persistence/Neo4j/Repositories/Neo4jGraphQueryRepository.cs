using backend.Application.Interfaces;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j;

public class Neo4jGraphQueryRepository : IGraphQueryRepository
{
    private readonly IDriver _driver;

    public Neo4jGraphQueryRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<IReadOnlyList<Guid>> GetRecommendedMastersAsync(Guid clientId, decimal? minRating = null, int limit = 10)
    {
        var query = @"
            MATCH (c:Client { id: $clientId })-[:HIRED]->(used:Master)-[:HAS_SKILL]->(s:Skill)
            MATCH (other:Master)-[:HAS_SKILL]->(s)
            WHERE other.id <> used.id
              AND NOT (c)-[:HIRED]->(other)
              AND ($minRating IS NULL OR other.rating >= $minRating)
            WITH other, max(other.rating) AS r
            RETURN other.id AS id
            ORDER BY coalesce(r, -1.0) DESC
            LIMIT $limit
        ";
        var parameters = new { clientId = clientId.ToString(), minRating = minRating.HasValue ? (double?)minRating.Value : null, limit };

        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, parameters);
            var list = new List<Guid>();
            while (await cursor.FetchAsync())
                list.Add(Guid.Parse(cursor.Current["id"].As<string>()));
            return list;
        });
    }

    public async Task<IReadOnlyList<Guid>> SearchMastersAsync(
        IReadOnlyList<string>? categoryNames = null,
        IReadOnlyList<string>? zoneIds = null,
        decimal? minRating = null,
        int limit = 20)
    {
        var categoryNameList = categoryNames ?? new List<string>();
        var zoneIdList = zoneIds ?? new List<string>();

        var query = @"
            MATCH (m:Master)
            WHERE ($minRating IS NULL OR m.rating >= $minRating)
            WITH m
            OPTIONAL MATCH (m)-[:HAS_SKILL]->(s:Skill)
            WITH m, collect(DISTINCT s.name) AS skills
            OPTIONAL MATCH (m)<-[:IS_MASTER]-(u:User)-[:LOCATED_IN]->(z:Zone)
            WITH m, skills, collect(DISTINCT z.id) AS zones
            WHERE (size($categoryNames) = 0 OR size([x IN skills WHERE x IN $categoryNames]) > 0)
              AND (size($zoneIds) = 0 OR size([x IN zones WHERE x IN $zoneIds]) > 0)
            RETURN m.id AS id
            ORDER BY coalesce(m.rating, -1.0) DESC
            LIMIT $limit
        ";
        var parameters = new
        {
            categoryNames = categoryNameList,
            zoneIds = zoneIdList,
            minRating = minRating.HasValue ? (double?)minRating.Value : null,
            limit
        };

        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, parameters);
            var list = new List<Guid>();
            while (await cursor.FetchAsync())
                list.Add(Guid.Parse(cursor.Current["id"].As<string>()));
            return list;
        });
    }
}
