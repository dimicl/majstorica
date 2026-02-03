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
        var node = UserGraphMapper.ToNode(userId, role);
        var parameters = UserGraphMapper.ToMergeParameters(node);

        var query = @"
            MERGE (u:User { id: $id })
            SET u.role = $role
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx => tx.RunAsync(query, parameters));
    }
}
