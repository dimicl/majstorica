using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class ConversationRepository : IConversationRepository
{
    private readonly IMongoCollection<ConversationDocument> _collection;

    public ConversationRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConversationDocument>("conversations");
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
}
