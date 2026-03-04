using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IUserGraphSync
{
    Task SyncUserNode(Guid userId, UserRole role);

    Task SyncMasterProfile(Guid masterUserId, MasterCategory? category, decimal? rating, int? yearsExperience);

    Task SyncUserZone(Guid userId, string zoneId, string zoneName);
}
