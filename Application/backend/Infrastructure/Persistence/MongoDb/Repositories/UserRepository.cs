using System.Text.RegularExpressions;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Bson;
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

    public async Task<List<User>> GetActiveMasters()
    {
        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(x => x.Role, UserRole.Master),
            Builders<UserDocument>.Filter.Eq(x => x.IsActive, true));
 
        var docs = await _collection
            .Find(filter)
            .SortBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync();
 
        return docs.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<List<User>> GetAll()
    {
        var docs = await _collection.Find(FilterDefinition<UserDocument>.Empty).ToListAsync();
        return docs.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<List<User>> GetByIds(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new List<User>();

        var filter = Builders<UserDocument>.Filter.In(x => x.Id, idList);
        var docs = await _collection.Find(filter).ToListAsync();
        return docs.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<List<User>> SearchMastersForCompanyInvite(
        string searchText,
        int limit,
        Guid excludeUserId)
    {
        var q = (searchText ?? string.Empty).Trim();
        if (q.Length < 2)
            return new List<User>();

        limit = Math.Clamp(limit, 1, 30);
        var escaped = Regex.Escape(q);
        var regex = new BsonRegularExpression(escaped, "i");

        var noEmployer = Builders<UserDocument>.Filter.Or(
            Builders<UserDocument>.Filter.Eq(x => x.EmployerCompanyId, null),
            Builders<UserDocument>.Filter.Exists(x => x.EmployerCompanyId, false));

        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(x => x.Role, UserRole.Master),
            Builders<UserDocument>.Filter.Eq(x => x.IsActive, true),
            noEmployer,
            Builders<UserDocument>.Filter.Ne(x => x.Id, excludeUserId),
            Builders<UserDocument>.Filter.Or(
                Builders<UserDocument>.Filter.Regex(x => x.FirstName, regex),
                Builders<UserDocument>.Filter.Regex(x => x.LastName, regex),
                Builders<UserDocument>.Filter.Regex(x => x.Username, regex),
                Builders<UserDocument>.Filter.Regex(x => x.Email, regex)));

        var docs = await _collection.Find(filter).Limit(limit).ToListAsync();
        return docs.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<List<User>> GetWorkersForCompany(Guid companyId)
    {
        if (companyId == Guid.Empty)
            return new List<User>();

        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(x => x.EmployerCompanyId, companyId),
            Builders<UserDocument>.Filter.Eq(x => x.Role, UserRole.CompanyWorker),
            Builders<UserDocument>.Filter.Eq(x => x.IsActive, true));

        var docs = await _collection
            .Find(filter)
            .SortBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync();

        return docs.Select(UserMapper.ToDomain).ToList();
    }
}
