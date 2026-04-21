using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class CompanyRepository : ICompanyRepository
{
    private readonly IMongoCollection<CompanyDocument> _collection;

    public CompanyRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<CompanyDocument>("companies");
    }

    public async Task<Company?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : CompanyMapper.ToDomain(doc);
    }

    public async Task<Company?> GetByOwnerUserId(Guid ownerUserId)
    {
        var doc = await _collection.Find(x => x.OwnerUserId == ownerUserId).FirstOrDefaultAsync();
        return doc == null ? null : CompanyMapper.ToDomain(doc);
    }

    public async Task<IReadOnlyList<Company>> GetAllActive()
    {
        var docs = await _collection.Find(x => x.IsActive).ToListAsync();
        return docs.Select(CompanyMapper.ToDomain).ToList();
    }

    public async Task Save(Company company)
    {
        var doc = CompanyMapper.ToDocument(company);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }
}
