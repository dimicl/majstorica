using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j;

public class Neo4jUserRepository : IUserRepository
{
    private readonly IDriver _driver;

    public Neo4jUserRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task Save(User user)
    {
        var query = @"
            MERGE (u:User { id: $id })
            SET u.firstName = $firstName,
                u.lastName = $lastName,
                u.email = $email,
                u.username = $username,
                u.passwordHash = $passwordHash,
                u.role = $role,
                u.isActive = $isActive
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx =>
            tx.RunAsync(query, new
            {
                id = user.Id.ToString(),
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                username = user.Username,
                passwordHash = user.PasswordHash,
                role = user.Role.ToString(),
                isActive = user.IsActive
            })
        );
    }

    public async Task<User?> GetById(Guid id)
    {
        var query = @"
            MATCH (u:User { id: $id })
            RETURN u
        ";

        await using var session = _driver.AsyncSession();

        var node = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { id = id.ToString() });
            if (!await cursor.FetchAsync())
                return null;

            return (INode)cursor.Current["u"];
        });

        return node == null ? null : User.Rehydrate(node);
    }

    public async Task<User?> GetByEmail(string email)
    {
        var query = @"
            MATCH (u:User { email: $email })
            RETURN u
        ";

        await using var session = _driver.AsyncSession();

        var node = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { email });
            if (!await cursor.FetchAsync())
                return null;

            return (INode)cursor.Current["u"];
        });

        return node == null ? null : User.Rehydrate(node);
    }

    public async Task<User?> GetByUsername(string username)
    {
        var query = @"
            MATCH (u:User { username: $username })
            RETURN u
        ";

        await using var session = _driver.AsyncSession();

        var node = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { username });
            if (!await cursor.FetchAsync())
                return null;

            return (INode)cursor.Current["u"];
        });

        return node == null ? null : User.Rehydrate(node);
    }

    public async Task<List<User>> GetAll()
    {
        var query = @"
            MATCH (u:User)
            RETURN u
        ";

        await using var session = _driver.AsyncSession();

        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query);
            var users = new List<User>();

            while (await cursor.FetchAsync())
            {
                var node = (INode)cursor.Current["u"];
                users.Add(User.Rehydrate(node));
            }

            return users;
        });
    }
}
