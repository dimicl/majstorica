using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class MongoJobRepository
{
    private readonly IMongoCollection<JobDocument> _collection;

    public MongoJobRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<JobDocument>("jobs");
    }

    public async Task Save(Job job)
    {
        var doc = JobMapper.ToDocument(job);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<Job?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : JobMapper.ToDomain(doc);
    }

    /// <summary>Poslovi gde je majstor dodeljen i status je jedan od navedenih.</summary>
    public async Task<List<Job>> GetByMasterIdAndStatuses(Guid masterId, IEnumerable<JobStatus> statuses)
    {
        var statusStrings = statuses.Select(s => s.ToString()).ToList();
        var filter = Builders<JobDocument>.Filter.And(
            Builders<JobDocument>.Filter.Eq(x => x.MasterId, masterId),
            Builders<JobDocument>.Filter.In(x => x.Status, statusStrings));
        var docs = await _collection.Find(filter).ToListAsync();
        return docs.Select(JobMapper.ToDomain).ToList();
    }

    /// <summary>Svi poslovi koje je kreirao klijent.</summary>
    public async Task<List<Job>> GetByClientId(Guid clientId)
    {
        var docs = await _collection
            .Find(x => x.ClientId == clientId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
        return docs.Select(JobMapper.ToDomain).ToList();
    }
}
