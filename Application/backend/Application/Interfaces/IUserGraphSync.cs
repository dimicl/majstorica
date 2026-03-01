using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IUserGraphSync
{
    Task SyncUserNode(Guid userId, UserRole role);

    /// <summary>Sinhronizuje Master čvor: rating, yearsExperience, HAS_SKILL prema kategoriji.</summary>
    Task SyncMasterProfile(Guid masterUserId, MasterCategory? category, decimal? rating, int? yearsExperience);

    /// <summary>Postavlja (User)-[:LOCATED_IN]->(Zone). Jedan korisnik = jedna zona (stara se zamenjuje).</summary>
    Task SyncUserZone(Guid userId, string zoneId, string zoneName);
}
