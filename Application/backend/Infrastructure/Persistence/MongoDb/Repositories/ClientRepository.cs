using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class ClientRepository : IClientRepository
{
    private readonly IMongoCollection<ClientDocument> _collection;

    public ClientRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ClientDocument>("clients");
    }

    public async Task Save(Guid userId, ClientProfile client)
    {
        var doc = ClientMapper.ToDocument(userId, client);
        await _collection.ReplaceOneAsync(
        x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
        }

    public async Task<ClientProfile?> GetById(Guid id)
    {
        return await GetByUserId(id);
    }

    public async Task<ClientProfile?> GetByUserId(Guid userId)
    {
        var doc = await _collection.Find(x => x.Id == userId).FirstOrDefaultAsync();
        return doc == null ? null : ClientMapper.ToDomain(doc);
    }
}
