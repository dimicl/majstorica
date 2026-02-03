using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IReviewRepository
{
    Task Save(Review review);
    Task<Review?> GetById(Guid id);
    Task<List<Review>> GetByMasterId(Guid masterId);
    Task<List<Review>> GetByJobId(Guid jobId);
}
