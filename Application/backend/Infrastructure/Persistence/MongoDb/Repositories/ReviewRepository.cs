using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using backend.Infrastructure.Persistence.MongoDb.Entities;
using MongoDB.Driver;

namespace backend.Infrastructure.Persistence.MongoDb;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<ReviewDocument> _collection;

    public ReviewRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ReviewDocument>("reviews");
    }

    public async Task Save(Review review)
    {
        var doc = new ReviewDocument
        {
            Id = review.Id,
            JobId = review.JobId,
            ReviewerUserId = review.ReviewerUserId,
            TargetType = review.TargetType,
            TargetMasterId = review.TargetMasterId,
            TargetCompanyId = review.TargetCompanyId,
            Rating = review.Rating.Value,
            Comment = review.Comment,
            IsEdited = review.IsEdited,
            CreatedAtUtc = review.CreatedAtUtc,
            UpdatedAtUtc = review.UpdatedAtUtc,
            EditedAtUtc = review.EditedAtUtc
        };

        await _collection.ReplaceOneAsync(
            x => x.Id == review.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<Review?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null
            ? null
            : new Review(
                doc.Id,
                doc.JobId,
                doc.ReviewerUserId,
                doc.TargetType,
                new Rating(doc.Rating),
                doc.CreatedAtUtc,
                doc.TargetMasterId,
                doc.TargetCompanyId,
                doc.Comment);
    }

    public async Task<List<Review>> GetByMasterId(Guid masterId)
    {
        var docs = await _collection
            .Find(x => x.TargetMasterId == masterId)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return docs.Select(d => new Review(
            d.Id,
            d.JobId,
            d.ReviewerUserId,
            d.TargetType,
            new Rating(d.Rating),
            d.CreatedAtUtc,
            d.TargetMasterId,
            d.TargetCompanyId,
            d.Comment)).ToList();
    }

    public async Task<List<Review>> GetByJobId(Guid jobId)
    {
        var docs = await _collection.Find(x => x.JobId == jobId).ToListAsync();
        return docs.Select(d => new Review(
            d.Id,
            d.JobId,
            d.ReviewerUserId,
            d.TargetType,
            new Rating(d.Rating),
            d.CreatedAtUtc,
            d.TargetMasterId,
            d.TargetCompanyId,
            d.Comment)).ToList();
    }
}
