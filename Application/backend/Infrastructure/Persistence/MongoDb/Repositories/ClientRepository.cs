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

    public async Task Save(Client client)
    {
        var doc = ClientMapper.ToDocument(client);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<Client?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : ClientMapper.ToDomain(doc);
    }

    public async Task<Client?> GetByUserId(Guid userId)
    {
        var doc = await _collection.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        return doc == null ? null : ClientMapper.ToDomain(doc);
    }
}
