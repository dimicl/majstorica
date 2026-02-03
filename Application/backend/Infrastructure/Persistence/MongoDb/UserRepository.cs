using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _collection;

    public UserRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<UserDocument>("users");
    }

    public async Task Save(User user)
    {
        var doc = UserMapper.ToDocument(user);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<User?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : UserMapper.ToDomain(doc);
    }

    public async Task<User?> GetByEmail(string email)
    {
        var doc = await _collection.Find(x => x.Email == email).FirstOrDefaultAsync();
        return doc == null ? null : UserMapper.ToDomain(doc);
    }

    public async Task<User?> GetByUsername(string username)
    {
        var doc = await _collection.Find(x => x.Username == username).FirstOrDefaultAsync();
        return doc == null ? null : UserMapper.ToDomain(doc);
    }

    public async Task<List<User>> GetAll()
    {
        var docs = await _collection.Find(FilterDefinition<UserDocument>.Empty).ToListAsync();
        return docs.Select(UserMapper.ToDomain).ToList();
    }
}
