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
    private readonly ICompanyRepository _companyRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IRedisListCache _redisListCache;
    private readonly IGraphQueryRepository _graphQueryRepository;
    private readonly IAccountActivityStore _accountActivity;

    public UserService(
        IUserRepository userRepository,
        IUserGraphSync userGraphSync,
        IMasterRepository masterRepository,
        ICompanyRepository companyRepository,
        IReviewRepository reviewRepository,
        IRedisListCache redisListCache,
        IGraphQueryRepository graphQueryRepository,
        IAccountActivityStore accountActivity)
    {
        _userRepository = userRepository;
        _userGraphSync = userGraphSync;
        _masterRepository = masterRepository;
        _companyRepository = companyRepository;
        _reviewRepository = reviewRepository;
        _redisListCache = redisListCache;
        _graphQueryRepository = graphQueryRepository;
        _accountActivity = accountActivity;
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
        try
        {
            await _accountActivity.RecordAsync(userId, "profile_update", "basic_info");
        }
        catch
        {
        }
    }

    public async Task UpdateContact(Guid userId, string? phone)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new NotFoundException("Korisnik nije pronađen.");

        if(phone != null) user.ChangeContact(phone);
        await _userRepository.Save(user);
        try
        {
            await _accountActivity.RecordAsync(userId, "profile_update", "contact");
        }
        catch
        {
        }
    }

    public async Task SetUserZone(Guid userId, string zoneId, string zoneName)
    {
        if (string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(zoneName))
            throw new ArgumentException("ZoneId i zoneName su obavezni.");
        await _userGraphSync.SyncUserZone(userId, zoneId.Trim(), zoneName.Trim());
        try
        {
            await _accountActivity.RecordAsync(userId, "profile_update", "zone");
        }
        catch
        {
        }
    }

    public async Task Deactivate(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new NotFoundException("Korisnik nije pronađen.");

        user.Deactivate();
        await _userRepository.Save(user);
        await _userGraphSync.SyncUserNode(user.Id, user.Role);
        try
        {
            await _accountActivity.RecordAsync(userId, "account_deactivate");
        }
        catch
        {
        }
    }

    public async Task Activate(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new NotFoundException("Korisnik nije pronađen.");

        user.Activate();
        await _userRepository.Save(user);
        await _userGraphSync.SyncUserNode(user.Id, user.Role);
        try
        {
            await _accountActivity.RecordAsync(userId, "account_activate");
        }
        catch
        {
        }
    }

    public async Task<MastersListPageResponse> GetMastersList(MastersListQuery? query = null)
    {
        query ??= new MastersListQuery();
        var key = RedisListCacheKey.Create(query);

        List<MasterListItemResponse> filtered;
        try
        {
            var cached = await _redisListCache.GetAsync(key);
            if (cached != null)
                filtered = cached;
            else
            {
                var list = await GetCombinedListFromDb();
                filtered = ApplyFilterAndSort(list, query);
                try
                {
                    await _redisListCache.SetAsync(key, filtered, RedisListCacheTtl);
                }
                catch
                {
                    // Redis nedostupan
                }
            }
        }
        catch
        {
            var list = await GetCombinedListFromDb();
            filtered = ApplyFilterAndSort(list, query);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = Math.Clamp(query.PageSize < 1 ? 12 : query.PageSize, 1, 50);
        var total = filtered.Count;
        var skip = (page - 1) * pageSize;
        var items = filtered.Skip(skip).Take(pageSize).ToList();

        return new MastersListPageResponse
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<List<MasterListItemResponse>> GetCombinedListFromDb()
    {
        var masters = await GetMasterRowsFromDb();
        var companies = await GetCompanyRowsFromDb();
        var combined = new List<MasterListItemResponse>(masters.Count + companies.Count);
        combined.AddRange(masters);
        combined.AddRange(companies);
        return combined;
    }

    private async Task<List<MasterListItemResponse>> GetMasterRowsFromDb()
    {
        var masterUsers = await _userRepository.GetActiveMasters();
        if (masterUsers.Count == 0)
            return new List<MasterListItemResponse>();

        var userIds = masterUsers.Select(u => u.Id).ToList();
        var masterByUserId = await _masterRepository.GetByUserIds(userIds);

        return masterUsers
            .Select(u =>
            {
                var master = masterByUserId.GetValueOrDefault(u.Id);
                var cats = master?.ServiceCategories;
                List<string>? catList = cats is { Count: > 0 }
                    ? cats.ToList()
                    : null;
                return new MasterListItemResponse
                {
                    Kind = "master",
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Username = u.Username,
                    Category = master?.ServiceCategories?.FirstOrDefault(),
                    Rating = master?.AverageRating?.Value,
                    ServiceCategories = catList
                };
            })
            .ToList();
    }

    private async Task<List<MasterListItemResponse>> GetCompanyRowsFromDb()
    {
        var companies = await _companyRepository.GetAllActive();
        if (companies.Count == 0)
            return new List<MasterListItemResponse>();

        return companies
            .Select(c =>
            {
                var cats = c.ServiceCategories.ToList();
                return new MasterListItemResponse
                {
                    Kind = "company",
                    Id = c.Id,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Username = string.Empty,
                    CompanyName = c.Name,
                    Description = c.Description,
                    City = c.Address?.City,
                    Email = c.Email,
                    Category = cats.FirstOrDefault(),
                    Rating = null,
                    ServiceCategories = cats.Count > 0 ? cats : null
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
        var entityType = (query.EntityType ?? "all").Trim().ToLowerInvariant();

        IEnumerable<MasterListItemResponse> result = list;

        result = entityType switch
        {
            "masters" => result.Where(m => string.Equals(m.Kind, "master", StringComparison.OrdinalIgnoreCase)),
            "companies" => result.Where(m => string.Equals(m.Kind, "company", StringComparison.OrdinalIgnoreCase)),
            _ => result
        };

        if (!string.IsNullOrEmpty(search))
        {
            result = result.Where(m =>
            {
                if (string.Equals(m.Kind, "company", StringComparison.OrdinalIgnoreCase))
                {
                    return (m.CompanyName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (m.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (m.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (m.City?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false);
                }

                return (m.FirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                       (m.LastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                       (m.Username?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false);
            });
        }

        if (!string.IsNullOrEmpty(category))
        {
            result = result.Where(m =>
            {
                if (m.ServiceCategories is { Count: > 0 })
                {
                    return m.ServiceCategories.Any(c =>
                        string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
                }

                return string.Equals(m.Category, category, StringComparison.OrdinalIgnoreCase);
            });
        }

        if (minRating.HasValue && minRating.Value >= 1 && minRating.Value <= 5)
        {
            result = result.Where(m =>
                string.Equals(m.Kind, "company", StringComparison.OrdinalIgnoreCase)
                    ? false
                    : m.Rating.HasValue && m.Rating.Value >= minRating.Value);
        }

        var sorted = result.OrderBy(GetSortDisplayName, StringComparer.OrdinalIgnoreCase).ToList();

        if (!sortAsc)
            sorted.Reverse();

        return sorted;
    }

    private static string GetSortDisplayName(MasterListItemResponse m)
    {
        if (string.Equals(m.Kind, "company", StringComparison.OrdinalIgnoreCase))
            return m.CompanyName ?? "";

        var name = $"{m.FirstName} {m.LastName}".Trim();
        if (string.IsNullOrEmpty(name)) name = m.Username ?? "";
        return name;
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

        var users = await _userRepository.GetByIds(ids);
        var userById = users.ToDictionary(u => u.Id);

        foreach (var id in ids)
        {
            
            if (!userById.TryGetValue(id, out var user) || !user.IsActive || user.Role != UserRole.Master)
                continue;

            var master = masterByUserId.GetValueOrDefault(id);
            var cats = master?.ServiceCategories;
            List<string>? catList = cats is { Count: > 0 }
                ? cats.ToList()
                : null;
            result.Add(new MasterListItemResponse
            {
                Kind = "master",
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Category = master?.ServiceCategories?.FirstOrDefault(),
                Rating = master?.AverageRating?.Value,
                ServiceCategories = catList
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

        var users = await _userRepository.GetByIds(ids);
        var userById = users.ToDictionary(u => u.Id);

        foreach (var id in ids)
        { 
            if (!userById.TryGetValue(id, out var user) || !user.IsActive || user.Role != UserRole.Master)
                continue;

            var master = masterByUserId.GetValueOrDefault(id);
            var cats = master?.ServiceCategories;
            List<string>? catList = cats is { Count: > 0 }
                ? cats.ToList()
                : null;
            result.Add(new MasterListItemResponse
            {
                Kind = "master",
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Category = master?.ServiceCategories?.FirstOrDefault(),
                Rating = master?.AverageRating?.Value,
                ServiceCategories = catList
            });
        }

        return result;
    }

    public async Task<MasterProfileResponse?> GetMasterProfile(Guid userId)
    {
        var userDto = await GetProfile(userId);
        if (userDto == null) return null;

        Guid? employerCompanyId = null;
        string? employerCompanyName = null;
        if (userDto.Role == UserRole.CompanyWorker)
        {
            var domainUser = await _userRepository.GetById(userId);
            var cid = domainUser?.EmployerCompanyId;
            if (cid is { } id && id != Guid.Empty)
            {
                employerCompanyId = id;
                var company = await _companyRepository.GetById(id);
                employerCompanyName = company?.Name;
            }
        }

        var master = await _masterRepository.GetByUserId(userId);
        return new MasterProfileResponse
        {
            User = userDto,
            Category = master?.ServiceCategories?.FirstOrDefault(),
            Rating = master?.AverageRating?.Value,
            EmployerCompanyId = employerCompanyId,
            EmployerCompanyName = employerCompanyName,
            YearsOfExperience = master?.YearsOfExperience ?? 0,
            HourlyRateAmount = master?.HourlyRate?.Amount ?? 0,
            HourlyRateCurrency = master?.HourlyRate?.Currency ?? "RSD",
            TotalReviews = master?.TotalReviews ?? 0
        };
    }

    public async Task<List<MasterReviewListItemResponse>> GetMasterReviews(Guid masterId)
    {
        var reviews = await _reviewRepository.GetByMasterId(masterId);
        var reviewerIds = reviews.Select(r => r.ReviewerUserId).Distinct().ToList();
        var reviewers = await _userRepository.GetByIds(reviewerIds);
        var reviewerById = reviewers.ToDictionary(u => u.Id);

        var result = new List<MasterReviewListItemResponse>(reviews.Count);
        foreach (var r in reviews)
        {
            reviewerById.TryGetValue(r.ReviewerUserId, out var reviewer);
            var name = reviewer != null
                ? $"{reviewer.FirstName} {reviewer.LastName}".Trim()
                : string.Empty;
            if (string.IsNullOrWhiteSpace(name)) name = "Korisnik";

            result.Add(new MasterReviewListItemResponse
            {
                Id = r.Id,
                JobId = r.JobId,
                Rating = r.Rating.Value,
                Comment = r.Comment,
                CreatedAtUtc = r.CreatedAtUtc,
                ReviewerName = name,
                ReviewerUsername = reviewer?.Username
            });
        }

        return result;
    }

    public async Task UpdateMasterProfileStats(Guid userId, int? yearsOfExperience, decimal? hourlyRateAmount, string? hourlyRateCurrency)
    {
        var master = await _masterRepository.GetByUserId(userId);
        if (master is null)
            throw new NotFoundException("Profil majstora nije pronađen.");

        if (yearsOfExperience.HasValue)
            master.UpdateYearsOfExperience(yearsOfExperience.Value);

        if (hourlyRateAmount.HasValue)
        {
            var cur = string.IsNullOrWhiteSpace(hourlyRateCurrency)
                ? "RSD"
                : hourlyRateCurrency.Trim().ToUpperInvariant();
            master.SetHourlyRate(new Domain.ValueObjects.Money(hourlyRateAmount.Value, cur));
        }

        await _masterRepository.Save(userId, master);
        await _userGraphSync.SyncMasterProfile(
            userId,
            master.ServiceCategories.FirstOrDefault(),
            master.AverageRating?.Value,
            master.YearsOfExperience);
    }

    public async Task UpdateMasterCategory(Guid userId, string? category)
    {
        var master = await _masterRepository.GetByUserId(userId);
        if (master is null)
            throw new NotFoundException("Profil majstora nije pronađen.");

        var categoryDisplayName = string.IsNullOrWhiteSpace(category) ? "Ostalo" : category.Trim();
        master.ReplaceServiceCategories(new[] { categoryDisplayName });
        await _masterRepository.Save(userId, master);
        await _userGraphSync.SyncMasterProfile(userId, categoryDisplayName, master.AverageRating?.Value, master.YearsOfExperience);
    }
}