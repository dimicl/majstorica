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
    private readonly IReviewRepository _reviewRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRedisListCache _redisListCache;
    private readonly IGraphQueryRepository _graphQueryRepository;

    public UserService(
        IUserRepository userRepository,
        IUserGraphSync userGraphSync,
        IMasterRepository masterRepository,
        IReviewRepository reviewRepository,
        ICompanyRepository companyRepository,
        IRedisListCache redisListCache,
        IGraphQueryRepository graphQueryRepository)
    {
        _userRepository = userRepository;
        _userGraphSync = userGraphSync;
        _masterRepository = masterRepository;
        _reviewRepository = reviewRepository;
        _companyRepository = companyRepository;
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
        var address = new AddressResponse
        {
            Street = user.Address?.Street ?? string.Empty,
            City = user.Address?.City ?? string.Empty,
            Zone = user.Address?.Zone ?? string.Empty,
            PostalCode = user.Address?.PostalCode ?? string.Empty,
            Country = user.Address?.Country ?? string.Empty
        };
        return new UserRequest
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Phone = user.PhoneNumber,
            Address = address
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

    public async Task<MastersListPageResponse> GetMastersList(MastersListQuery? query = null)
    {
        query ??= new MastersListQuery();
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 12 : query.PageSize;
        var key = RedisListCacheKey.Create(query);

        List<MasterListItemResponse>? filtered = null;
        try
        {
            var cached = await _redisListCache.GetAsync(key);
            if (cached != null)
                filtered = cached;
        }
        catch
        {
            // Redis nedostupan – nastavljamo bez keša
        }

        if (filtered == null)
        {
            var list = await GetMastersListFromDb();
            filtered = ApplyFilterAndSort(list, query);

            try
            {
                await _redisListCache.SetAsync(key, filtered, RedisListCacheTtl);
            }
            catch
            {
                // Redis nedostupan – ignorišemo, podaci su već učitani
            }
        }

        var totalCount = filtered.Count;
        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new MastersListPageResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<List<MasterListItemResponse>> GetMastersListFromDb()
    {
        var users = await _userRepository.GetAll();
        var masterUsers = users
            .Where(u =>
                (u.Role == UserRole.Master /* || u.Role == UserRole.CompanyWorker */) && u.IsActive)
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
            if (user == null || !user.IsActive)
                continue;
            if (user.Role != UserRole.Master && user.Role != UserRole.CompanyWorker)
                continue;
            var master = masterByUserId.GetValueOrDefault(id);
            if (user.Role == UserRole.CompanyWorker && master == null)
                continue;
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
            if (user == null || !user.IsActive)
                continue;
            if (user.Role != UserRole.Master && user.Role != UserRole.CompanyWorker)
                continue;
            var master = masterByUserId.GetValueOrDefault(id);
            if (user.Role == UserRole.CompanyWorker && master == null)
                continue;
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

    public async Task<MasterProfileResponse?> GetMasterProfile(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            return null;

        var profile = await GetProfile(userId);
        if (profile == null)
            return null;

        var master = await _masterRepository.GetByUserId(userId);
        string? employerCompanyName = null;
        if (user.EmployerCompanyId.HasValue)
        {
            var company = await _companyRepository.GetById(user.EmployerCompanyId.Value);
            employerCompanyName = company?.Name;
        }

        return new MasterProfileResponse
        {
            User = profile,
            Category = master?.ServiceCategories?.FirstOrDefault(),
            Rating = master?.AverageRating?.Value,
            EmployerCompanyId = user.EmployerCompanyId,
            EmployerCompanyName = employerCompanyName,
            YearsOfExperience = master?.YearsOfExperience ?? 0,
            HourlyRateAmount = master?.HourlyRate?.Amount ?? 0m,
            HourlyRateCurrency = master?.HourlyRate?.Currency ?? "RSD",
            TotalReviews = master?.TotalReviews ?? 0
        };
    }

    public async Task<List<MasterReviewListItemResponse>> GetMasterReviews(Guid masterId)
    {
        var reviews = await _reviewRepository.GetByMasterId(masterId);
        var items = new List<MasterReviewListItemResponse>();

        foreach (var review in reviews)
        {
            var reviewer = await _userRepository.GetById(review.ReviewerUserId);
            items.Add(new MasterReviewListItemResponse
            {
                Id = review.Id,
                JobId = review.JobId,
                Rating = review.Rating.Value,
                Comment = review.Comment,
                CreatedAtUtc = review.CreatedAtUtc,
                ReviewerName = reviewer != null
                    ? UserDisplayNameHelper.GetDisplayName(reviewer, "Korisnik")
                    : "Korisnik",
                ReviewerUsername = reviewer?.Username
            });
        }

        return items;
    }

    public async Task UpdateMasterProfileStats(
        Guid userId,
        int? yearsOfExperience,
        decimal? hourlyRateAmount,
        string? hourlyRateCurrency)
    {
        var user = await _userRepository.GetById(userId)
            ?? throw new NotFoundException("Korisnik nije pronađen.");
        var master = await _masterRepository.GetByUserId(userId)
            ?? throw new NotFoundException("Profil majstora nije pronađen.");

        if (yearsOfExperience.HasValue)
            master.UpdateYearsOfExperience(yearsOfExperience.Value);
        if (hourlyRateAmount.HasValue)
            master.SetHourlyRate(new Domain.ValueObjects.Money(
                hourlyRateAmount.Value,
                string.IsNullOrWhiteSpace(hourlyRateCurrency) ? "RSD" : hourlyRateCurrency.Trim()));

        user.SetMasterProfile(master);
        await _userRepository.Save(user);
        await _userGraphSync.SyncMasterProfile(
            user.Id,
            master.ServiceCategories.FirstOrDefault(),
            master.AverageRating?.Value,
            master.YearsOfExperience);
    }

    public async Task UpdateMasterCategory(Guid userId, string? category)
    {
        var user = await _userRepository.GetById(userId)
            ?? throw new NotFoundException("Korisnik nije pronađen.");
        var master = await _masterRepository.GetByUserId(userId)
            ?? throw new NotFoundException("Profil majstora nije pronađen.");

        var categoryDisplayName = string.IsNullOrWhiteSpace(category)
            ? "Ostalo"
            : category.Trim();
        master.ReplaceServiceCategories(new[] { categoryDisplayName });

        user.SetMasterProfile(master);
        await _userRepository.Save(user);
        await _userGraphSync.SyncMasterProfile(
            user.Id,
            categoryDisplayName,
            master.AverageRating?.Value,
            master.YearsOfExperience);
    }
}
