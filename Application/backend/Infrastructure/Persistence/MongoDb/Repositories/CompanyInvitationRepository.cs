using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using backend.Infrastructure.Persistence.MongoDb.Mappers;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class CompanyInvitationRepository : ICompanyInvitationRepository
{
    private readonly IMongoCollection<CompanyInvitationDocument> _collection;

    public CompanyInvitationRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<CompanyInvitationDocument>("company_invitations");
    }

    public async Task<CompanyInvitation?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : CompanyInvitationMapper.ToDomain(doc);
    }

    public async Task<CompanyInvitation?> GetPendingByCompanyAndMaster(Guid companyId, Guid masterUserId)
    {
        var doc = await _collection
            .Find(x =>
                x.CompanyId == companyId &&
                x.MasterUserId == masterUserId &&
                x.Status == CompanyInvitationStatus.Pending)
            .FirstOrDefaultAsync();
        return doc == null ? null : CompanyInvitationMapper.ToDomain(doc);
    }

    public async Task<List<CompanyInvitation>> GetPendingForMaster(Guid masterUserId)
    {
        var docs = await _collection
            .Find(x => x.MasterUserId == masterUserId && x.Status == CompanyInvitationStatus.Pending)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
        return docs.Select(CompanyInvitationMapper.ToDomain).ToList();
    }

    public async Task<List<CompanyInvitation>> GetPendingForCompany(Guid companyId)
    {
        var docs = await _collection
            .Find(x => x.CompanyId == companyId && x.Status == CompanyInvitationStatus.Pending)
            .ToListAsync();
        return docs.Select(CompanyInvitationMapper.ToDomain).ToList();
    }

    public async Task Save(CompanyInvitation invitation)
    {
        var doc = CompanyInvitationMapper.ToDocument(invitation);
        await _collection.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }
}
