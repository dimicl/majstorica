using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class MasterRepository : IMasterRepository
{
    private readonly IMongoCollection<MasterDocument> _collection;

    public MasterRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<MasterDocument>("masters");
    }

    public async Task Save(Master master)
    {
        var masterEntity = MasterMapper.ToEntity(master);
        await _collection.ReplaceOneAsync(
            x => x.Id == masterEntity.Id,
            masterEntity,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<Master?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : MasterMapper.ToDomain(doc);
    }

    public async Task<Master?> GetByUserId(Guid userId)
    {
        var doc = await _collection.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        return doc == null ? null : MasterMapper.ToDomain(doc);
    }

    public async Task<List<Master>> GetByUserIds(IEnumerable<Guid> userIds)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new List<Master>();
        var filter = Builders<MasterDocument>.Filter.In(x => x.UserId, ids);
        var docs = await _collection.Find(filter).ToListAsync();
        return docs.Select(MasterMapper.ToDomain).ToList();
    }
}
