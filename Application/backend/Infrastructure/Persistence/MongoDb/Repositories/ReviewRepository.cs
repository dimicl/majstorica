using backend.Application.Interfaces;
using backend.Domain.Entities;
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
            ClientId = review.ClientId,
            MasterId = review.MasterId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
        await _collection.ReplaceOneAsync(
            x => x.Id == review.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<Review?> GetById(Guid id)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? null : Review.Rehydrate(doc.Id, doc.JobId, doc.ClientId, doc.MasterId, doc.Rating, doc.Comment, doc.CreatedAt);
    }

    public async Task<List<Review>> GetByMasterId(Guid masterId)
    {
        var docs = await _collection.Find(x => x.MasterId == masterId).SortByDescending(x => x.CreatedAt).ToListAsync();
        return docs.Select(d => Review.Rehydrate(d.Id, d.JobId, d.ClientId, d.MasterId, d.Rating, d.Comment, d.CreatedAt)).ToList();
    }

    public async Task<List<Review>> GetByJobId(Guid jobId)
    {
        var docs = await _collection.Find(x => x.JobId == jobId).ToListAsync();
        return docs.Select(d => Review.Rehydrate(d.Id, d.JobId, d.ClientId, d.MasterId, d.Rating, d.Comment, d.CreatedAt)).ToList();
    }

    private class ReviewDocument
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ClientId { get; set; }
        public Guid MasterId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
