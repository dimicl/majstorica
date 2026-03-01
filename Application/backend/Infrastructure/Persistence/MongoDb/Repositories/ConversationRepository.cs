using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;
using StackExchange.Redis;

namespace backend.Infrastructure.Persistence.MongoDb;

public class ConversationRepository : IConversationRepository
{
    private readonly IMongoCollection<ConversationDocument> _collection;
    private readonly IDatabase _redis;

    private static string UnreadKey(Guid conversationId, Guid userId) =>
        $"unread:{conversationId}:{userId}";

    public ConversationRepository(IMongoDatabase database, IConnectionMultiplexer redis)
    {
        _collection = database.GetCollection<ConversationDocument>("conversations");
        _redis = redis.GetDatabase();
    }

    public async Task Save(ChatConversation conversation)
    {
        var doc = ConversationMapper.ToDocument(conversation);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
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
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : ConversationMapper.ToDomain(doc);
    }

    public async Task<List<ChatConversation>> GetByJobId(Guid jobId)
    {
        var docs = await _collection.Find(x => x.JobId == jobId).ToListAsync();
        return docs.Select(ConversationMapper.ToDomain).ToList();
    }

    public async Task<List<ChatConversation>> GetByUserId(Guid userId)
    {
        var filter = Builders<ConversationDocument>.Filter.Or(
            Builders<ConversationDocument>.Filter.Eq(x => x.ClientId, userId),
            Builders<ConversationDocument>.Filter.Eq(x => x.MasterId, userId)
        );
        var docs = await _collection.Find(filter).ToListAsync();
        return docs.Select(ConversationMapper.ToDomain).ToList();
    }

    public async Task<ChatConversation?> GetActiveByClientAndMaster(Guid clientId, Guid masterId)
    {
        var doc = await _collection
            .Find(x => x.ClientId == clientId && x.MasterId == masterId && x.IsActive)
            .FirstOrDefaultAsync();
        return doc == null ? null : ConversationMapper.ToDomain(doc);
    }

    public async Task<ChatConversation?> GetByClientAndMaster(Guid clientId, Guid masterId)
    {
        var doc = await _collection
            .Find(x => x.ClientId == clientId && x.MasterId == masterId)
            .FirstOrDefaultAsync();
        return doc == null ? null : ConversationMapper.ToDomain(doc);
    }

    public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
    {
        var val = await _redis.StringGetAsync(UnreadKey(conversationId, userId));
        return val.IsNullOrEmpty || !long.TryParse(val, out var n) ? 0 : (int)Math.Min(n, int.MaxValue);
    }

    public Task MarkAsReadAsync(Guid conversationId, Guid userId) =>
        _redis.StringSetAsync(UnreadKey(conversationId, userId), 0);

    public Task IncrementUnreadAsync(Guid conversationId, Guid userId) =>
        _redis.StringIncrementAsync(UnreadKey(conversationId, userId));
}
