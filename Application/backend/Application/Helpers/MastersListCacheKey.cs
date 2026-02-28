using backend.Api.DTOs.Master;

namespace backend.Application.Helpers;

public static class RedisListCacheKey
{
    private const string Prefix = "masters:list:";

    public static string Create(MastersListQuery query)
    {
        if (query == null)
            return Prefix + "search=&sort=name-asc&category=&minRating=";

        var search = (query.Search ?? "").Trim().ToLowerInvariant();
        var sort = string.IsNullOrWhiteSpace(query.Sort) ? "name-asc" : query.Sort.Trim();
        var category = (query.Category ?? "").Trim();
        var minRating = query.MinRating?.ToString() ?? "";

        var suffix = $"search={Uri.EscapeDataString(search)}&sort={sort}&category={Uri.EscapeDataString(category)}&minRating={minRating}";
        return Prefix + suffix;
    }
}
