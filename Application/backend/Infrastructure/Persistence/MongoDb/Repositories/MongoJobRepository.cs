using backend.Application.Interfaces;
using backend.Domain.Entities;
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
}
