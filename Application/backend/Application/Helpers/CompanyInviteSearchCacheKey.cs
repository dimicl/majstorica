namespace backend.Application.Helpers;

public static class CompanyInviteSearchCacheKey
{
    public static string Create(Guid ownerUserId, string query, int limit) =>
        $"cache:company:invite-search:{ownerUserId:D}:{limit}:{query.Trim().ToLowerInvariant()}";
}
