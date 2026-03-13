using backend.Application.Interfaces;
using backend.Domain.Enums;
using backend.Infrastructure.Persistence.Neo4j.Mappers;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j;

public class Neo4jUserGraphRepository : IUserGraphSync
{
    private readonly IDriver _driver;

    public Neo4jUserGraphRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task SyncUserNode(Guid userId, UserRole role)
    {
        var id = userId.ToString();
        var parameters = new { id };

        string query = role switch
        {
            UserRole.Client => @"
                MERGE (u:User { id: $id })
                MERGE (c:Client { id: $id })
                MERGE (u)-[:IS_CLIENT]->(c)
            ",
            UserRole.Master => @"
                MERGE (u:User { id: $id })
                MERGE (m:Master { id: $id })
                MERGE (u)-[:IS_MASTER]->(m)
            ",
            _ => @"
                MERGE (u:User { id: $id })
            "
        };

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx => tx.RunAsync(query, parameters));
    }

    public async Task SyncMasterProfile(Guid masterUserId, string? categoryDisplayName, decimal? rating, int? yearsExperience)
    {
        var id = masterUserId.ToString();
        await using var session = _driver.AsyncSession();

        await session.ExecuteWriteAsync(async tx =>
        {
            var mergeMaster = @"
                MERGE (m:Master { id: $id })
                SET m.rating = $rating, m.yearsExperience = $yearsExperience
            ";
            var masterParams = new Dictionary<string, object?>
            {
                ["id"] = id,
                ["rating"] = rating.HasValue ? (double)rating.Value : (double?)null,
                ["yearsExperience"] = yearsExperience ?? (long?)null
            };
            await tx.RunAsync(mergeMaster, masterParams);

            if (!string.IsNullOrWhiteSpace(categoryDisplayName))
            {
                var skillName = categoryDisplayName.Trim();
                var mergeSkill = @"
                    MATCH (m:Master { id: $id })
                    MERGE (s:Skill { name: $skillName })
                    MERGE (m)-[:HAS_SKILL]->(s)
                ";
                await tx.RunAsync(mergeSkill, new { id, skillName });
            }
            else
            {
                var removeSkill = @"
                    MATCH (m:Master { id: $id })-[r:HAS_SKILL]->()
                    DELETE r
                ";
                await tx.RunAsync(removeSkill, new { id });
            }
        });
    }

    public async Task SyncUserZone(Guid userId, string zoneId, string zoneName)
    {
        var userIdStr = userId.ToString();
        var query = @"
            MERGE (z:Zone { id: $zoneId })
            SET z.name = $zoneName
            WITH z
            MERGE (u:User { id: $userId })
            WITH u, z
            OPTIONAL MATCH (u)-[r:LOCATED_IN]->()
            DELETE r
            WITH u, z
            MERGE (u)-[:LOCATED_IN]->(z)
        ";
        var parameters = new { userId = userIdStr, zoneId, zoneName };

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx => tx.RunAsync(query, parameters));
    }
}
