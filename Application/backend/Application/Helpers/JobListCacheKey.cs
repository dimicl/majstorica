using backend.Domain.Enums;

namespace backend.Application.Helpers;

public static class JobListCacheKey
{
    public static string ForUser(Guid userId, UserRole role) =>
        $"cache:jobs:user:{userId:D}:{role}";

    public static string Marketplace(int page, int pageSize) =>
        $"cache:jobs:marketplace:{page}:{pageSize}";
}
