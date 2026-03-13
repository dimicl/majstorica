using backend.Api.DTOs.Master;
using backend.Api.DTOs.User;
using backend.Application.Helpers;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Application.Services;

public class UserService : IUserService
{
    private static readonly TimeSpan RedisListCacheTtl = TimeSpan.FromMinutes(5);

    private readonly IUserRepository _userRepository;
    private readonly IUserGraphSync _userGraphSync;
    private readonly IMasterRepository _masterRepository;
    private readonly IRedisListCache _redisListCache;
    private readonly IGraphQueryRepository _graphQueryRepository;

    public UserService(
        IUserRepository userRepository,
        IUserGraphSync userGraphSync,
        IMasterRepository masterRepository,
        IRedisListCache redisListCache,
        IGraphQueryRepository graphQueryRepository)
    {
        _userRepository = userRepository;
        _userGraphSync = userGraphSync;
        _masterRepository = masterRepository;
        _redisListCache = redisListCache;
        _graphQueryRepository = graphQueryRepository;
    }

    public async Task<User?> GetById(Guid userId)
    {
        return await _userRepository.GetById(userId);
    }

    public async Task<UserRequest?> GetProfile(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null) return null;
        return new UserRequest
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Phone = user.PhoneNumber,
            DeliveryAddress = user.Address?.ToString()
        };
    }

    public async Task UpdateProfile(
        Guid userId,
        string firstName,
        string lastName)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new NotFoundException("Korisnik nije pronađen.");

        user.UpdateBasicInfo(firstName, lastName, null);

        await _userRepository.Save(user);
        await _userGraphSync.SyncUserNode(user.Id, user.Role);
    }

    public async Task UpdateContact(Guid userId, string? phone)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new NotFoundException("Korisnik nije pronađen.");

        if(phone != null) user.ChangeContact(phone);
        await _userRepository.Save(user);
    }

    public async Task SetUserZone(Guid userId, string zoneId, string zoneName)
    {
        if (string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(zoneName))
            throw new ArgumentException("ZoneId i zoneName su obavezni.");
        await _userGraphSync.SyncUserZone(userId, zoneId.Trim(), zoneName.Trim());
    }

    public async Task Deactivate(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new NotFoundException("Korisnik nije pronađen.");

        user.Deactivate();
        await _userRepository.Save(user);
        await _userGraphSync.SyncUserNode(user.Id, user.Role);
    }

    public async Task Activate(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new NotFoundException("Korisnik nije pronađen.");

        user.Activate();
        await _userRepository.Save(user);
        await _userGraphSync.SyncUserNode(user.Id, user.Role);
    }

    public async Task<List<MasterListItemResponse>> GetMastersList(MastersListQuery? query = null)
    {
        query ??= new MastersListQuery();
        var key = RedisListCacheKey.Create(query);

        try
        {
            var cached = await _redisListCache.GetAsync(key);
            if (cached != null)
                return cached;
        }
        catch
        {
            // Redis nedostupan – nastavljamo bez keša
        }

        var list = await GetMastersListFromDb();
        var filtered = ApplyFilterAndSort(list, query);

        try
        {
            await _redisListCache.SetAsync(key, filtered, RedisListCacheTtl);
        }
        catch
        {
            // Redis nedostupan – ignorišemo, podaci su već učitani
        }

        return filtered;
    }

    private async Task<List<MasterListItemResponse>> GetMastersListFromDb()
    {
        var users = await _userRepository.GetAll();
        var masterUsers = users
            .Where(u => u.Role == UserRole.Master && u.IsActive)
            .ToList();
        if (masterUsers.Count == 0)
            return new List<MasterListItemResponse>();

        var userIds = masterUsers.Select(u => u.Id).ToList();
        var masterByUserId = await _masterRepository.GetByUserIds(userIds);

        return masterUsers
            .Select(u =>
            {
                var master = masterByUserId.GetValueOrDefault(u.Id);
                return new MasterListItemResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Username = u.Username,
                    Category = master?.ServiceCategories?.FirstOrDefault(),
                    Rating = master?.AverageRating?.Value
                };
            })
            .ToList();
    }

    private static List<MasterListItemResponse> ApplyFilterAndSort(List<MasterListItemResponse> list, MastersListQuery query)
    {
        var search = (query.Search ?? "").Trim().ToLowerInvariant();
        var category = (query.Category ?? "").Trim();
        var minRating = query.MinRating;
        var sortAsc = string.Equals(query.Sort, "name-desc", StringComparison.OrdinalIgnoreCase) ? false : true;

        IEnumerable<MasterListItemResponse> result = list;

        if (!string.IsNullOrEmpty(search))
        {
            result = result.Where(m =>
                (m.FirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.LastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Username?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrEmpty(category))
            result = result.Where(m => string.Equals(m.Category, category, StringComparison.OrdinalIgnoreCase));

        if (minRating.HasValue && minRating.Value >= 1 && minRating.Value <= 5)
            result = result.Where(m => m.Rating.HasValue && m.Rating.Value >= minRating.Value);

        var sorted = result.OrderBy(m =>
        {
            var name = $"{m.FirstName} {m.LastName}".Trim();
            if (string.IsNullOrEmpty(name)) name = m.Username ?? "";
            return name;
        }, StringComparer.OrdinalIgnoreCase).ToList();

        if (!sortAsc)
            sorted.Reverse();

        return sorted;
    }

    public async Task<List<MasterListItemResponse>> GetRecommendedMasters(Guid clientId, int limit = 10)
    {
        IReadOnlyList<Guid> ids;
        try
        {
            ids = await _graphQueryRepository.GetRecommendedMastersAsync(clientId, null, limit);
        }
        catch
        {
            // Neo4j nedostupan ili prazan graf – vraćamo praznu listu
            return new List<MasterListItemResponse>();
        }

        if (ids.Count == 0)
            return new List<MasterListItemResponse>();

        var result = new List<MasterListItemResponse>();
        var masterByUserId = await _masterRepository.GetByUserIds(ids.ToList());

        foreach (var id in ids)
        {
            var user = await _userRepository.GetById(id);
            if (user == null || !user.IsActive || user.Role != UserRole.Master)
                continue;
            var master = masterByUserId.GetValueOrDefault(id);
            result.Add(new MasterListItemResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Category = master?.ServiceCategories?.FirstOrDefault(),
                Rating = master?.AverageRating?.Value
            });
        }

        return result;
    }

    public async Task<List<MasterListItemResponse>> GetMastersByGraphSearch(
        IReadOnlyList<string>? categoryNames = null,
        IReadOnlyList<string>? zoneIds = null,
        decimal? minRating = null,
        int limit = 20)
    {
        IReadOnlyList<Guid> ids;
        try
        {
            ids = await _graphQueryRepository.SearchMastersAsync(categoryNames, zoneIds, minRating, limit);
        }
        catch
        {
            return new List<MasterListItemResponse>();
        }

        if (ids.Count == 0)
            return new List<MasterListItemResponse>();

        var result = new List<MasterListItemResponse>();
        var masterByUserId = await _masterRepository.GetByUserIds(ids.ToList());

        foreach (var id in ids)
        {
            var user = await _userRepository.GetById(id);
            if (user == null || !user.IsActive || user.Role != UserRole.Master)
                continue;
            var master = masterByUserId.GetValueOrDefault(id);
            result.Add(new MasterListItemResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Category = master?.ServiceCategories?.FirstOrDefault(),
                Rating = master?.AverageRating?.Value
            });
        }

        return result;
    }
}
