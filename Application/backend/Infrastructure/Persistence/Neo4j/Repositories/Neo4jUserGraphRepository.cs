using backend.Application.Interfaces;
using backend.Domain.Enums;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j;

public class Neo4jUserGraphRepository : IUserGraphSync
{
    private readonly IDriver _driver;

    public Neo4jUserGraphRepository(IDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Kreira User čvor, zatim Client ili Master čvor (inicijalno prazan) i povezuje ga sa User-om.
    /// Ostala svojstva na Client/Master će se dodavati kasnije na profilu.
    /// </summary>
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
}
