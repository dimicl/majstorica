using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class MongoMessageRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageDocument> _collection;

    public MongoMessageRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<MessageDocument>("messages");
    }

    public async Task Save(ChatMessage message)
    {
        var doc = MessageMapper.ToDocument(message);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<List<ChatMessage>> GetByConversationId(Guid conversationId)
    {
        var docs = await _collection
            .Find(x => x.ConversationId == conversationId)
            .SortBy(x => x.SentAt)
            .ToListAsync();
        return docs.Select(MessageMapper.ToDomain).ToList();
    }

    public async Task<ChatMessage?> GetLastByConversationId(Guid conversationId)
    {
        var doc = await _collection
            .Find(x => x.ConversationId == conversationId)
            .SortByDescending(x => x.SentAt)
            .FirstOrDefaultAsync();
        return doc == null ? null : MessageMapper.ToDomain(doc);
    }

}
