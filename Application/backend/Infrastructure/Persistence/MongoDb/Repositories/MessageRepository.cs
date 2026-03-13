using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class MessageRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageDocument> _collection;

    public MessageRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<MessageDocument>("messages");
    }

    public async Task Save(Message message)
    {
        var doc = MessageMapper.ToDocument(message);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<List<Message>> GetByConversationId(Guid conversationId)
    {
        var docs = await _collection
            .Find(x => x.ConversationId == conversationId)
            .SortBy(x => x.SentAtUtc)
            .ToListAsync();
        return docs.Select(MessageMapper.ToDomain).ToList();
    }

    public async Task<Message?> GetLastByConversationId(Guid conversationId)
    {
        var doc = await _collection
            .Find(x => x.ConversationId == conversationId)
            .SortByDescending(x => x.SentAtUtc)
            .FirstOrDefaultAsync();
        return doc == null ? null : MessageMapper.ToDomain(doc);
    }

}
