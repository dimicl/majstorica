namespace backend.Application.Interfaces;

/// Samo čitanje iz Neo4j grafa: preporuke i složene pretrage majstora (po kategoriji, zoni, oceni).
public interface IGraphQueryRepository
{
    Task<IReadOnlyList<Guid>> GetRecommendedMastersAsync(Guid clientId, decimal? minRating = null, int limit = 10);

    Task<IReadOnlyList<Guid>> SearchMastersAsync(
        IReadOnlyList<string>? categoryNames = null,
        IReadOnlyList<string>? zoneIds = null,
        decimal? minRating = null,
        int limit = 20);
}
