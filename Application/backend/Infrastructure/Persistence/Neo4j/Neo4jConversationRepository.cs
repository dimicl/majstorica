using backend.Application.Interfaces;
using backend.Domain.Entities;
using Neo4j.Driver;

namespace backend.Infrastructure.Persistence.Neo4j;

public class Neo4jConversationRepository : IConversationRepository
{
    private readonly IDriver _driver;

    public Neo4jConversationRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task Save(ChatConversation conversation)
    {
        var query = @"
            MERGE (c:Conversation { id: $id })
            SET c.jobId = $jobId,
                c.clientId = $clientId,
                c.masterId = $masterId,
                c.isActive = $isActive
        ";

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(tx =>
            tx.RunAsync(query, new
            {
                id = conversation.Id.ToString(),
                jobId = conversation.JobId.ToString(),
                clientId = conversation.ClientId.ToString(),
                masterId = conversation.MasterId.ToString(),
                isActive = conversation.IsActive
            })
        );
    }

    public async Task SaveMany(IEnumerable<ChatConversation> conversations)
    {
        foreach (var conversation in conversations)
        {
            await Save(conversation);
        }
    }

    public async Task<ChatConversation?> GetById(Guid id)
    {
        var query = @"
            MATCH (c:Conversation { id: $id })
            RETURN c
        ";

        await using var session = _driver.AsyncSession();

        var node = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { id = id.ToString() });
            if (!await cursor.FetchAsync())
                return null;

            return (INode)cursor.Current["c"];
        });

        return node == null ? null : ChatConversation.Rehydrate(node);
    }

    public async Task<List<ChatConversation>> GetByJobId(Guid jobId)
    {
        var query = @"
            MATCH (c:Conversation { jobId: $jobId })
            RETURN c
        ";

        await using var session = _driver.AsyncSession();

        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { jobId = jobId.ToString() });
            var list = new List<ChatConversation>();

            while (await cursor.FetchAsync())
            {
                var node = (INode)cursor.Current["c"];
                list.Add(ChatConversation.Rehydrate(node));
            }

            return list;
        });
    }
}
