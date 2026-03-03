using backend.Api.DTOs.Master;
using backend.Api.DTOs.User;
using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IUserService
{
    Task<User?> GetById(Guid userId);

    Task<UserRequest?> GetProfile(Guid userId);

    Task UpdateProfile(
        Guid userId,
        string firstName,
        string lastName);

    Task UpdateContact(Guid userId, string? phone, string? deliveryAddress);

    /// <summary>Postavlja zonu korisnika u Neo4j (za pretragu po lokaciji). Ne čuva u MongoDB.</summary>
    Task SetUserZone(Guid userId, string zoneId, string zoneName);

    Task Deactivate(Guid userId);
    Task Activate(Guid userId);

    Task<List<MasterListItemResponse>> GetMastersList(MastersListQuery? query = null);

    /// <summary>Preporučeni majstori za klijenta (Neo4j: ista veština kao već angažovani). Vraća praznu listu ako nema preporuka.</summary>
    Task<List<MasterListItemResponse>> GetRecommendedMasters(Guid clientId, int limit = 10);

    /// <summary>Složena pretraga majstora iz Neo4j grafa (kategorija, zona, min ocena). Rezultat iz grafa + podaci iz MongoDB.</summary>
    Task<List<MasterListItemResponse>> GetMastersByGraphSearch(
        IReadOnlyList<int>? categoryIds = null,
        IReadOnlyList<string>? zoneIds = null,
        decimal? minRating = null,
        int limit = 20);
}
