using backend.Api.DTOs.Master;
using backend.Api.Extensions;
using backend.Application.Interfaces;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/masters")]
[Authorize]
public class MastersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMasterRepository _masterRepository;
    private readonly IUserGraphSync _userGraphSync;
    private readonly ICompanyRepository _companyRepository;
    private readonly IReviewRepository _reviewRepository;

    public MastersController(
        IUserService userService,
        IMasterRepository masterRepository,
        IUserGraphSync userGraphSync,
        ICompanyRepository companyRepository,
        IReviewRepository reviewRepository)
    {
        _userService = userService;
        _masterRepository = masterRepository;
        _userGraphSync = userGraphSync;
        _companyRepository = companyRepository;
        _reviewRepository = reviewRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<MasterListItemResponse>>> GetMasters([FromQuery] MastersListQuery? query = null)
    {
        var result = await _userService.GetMastersList(query);
        return Ok(result);
    }

    /// <summary>Složena pretraga majstora iz Neo4j grafa (kategorija, zona, min ocena). categoryIds=1,2 zoneIds=id1,id2 minRating=4 limit=20.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<MasterListItemResponse>>> SearchByGraph([FromQuery] MastersGraphSearchQuery? query = null)
    {
        query ??= new MastersGraphSearchQuery();
        var categoryNames = ParseStringList(query.CategoryNames);
        var zoneIds = ParseStringList(query.ZoneIds);
        var limit = Math.Clamp(query.Limit, 1, 50);
        var result = await _userService.GetMastersByGraphSearch(
            categoryNames.Count > 0 ? categoryNames : null,
            zoneIds.Count > 0 ? zoneIds : null,
            query.MinRating,
            limit);
        return Ok(result);
    }

    /// <summary>Preporučeni majstori za trenutnog klijenta (Neo4j: ista veština kao već angažovani). Za ne-klijente vraća praznu listu.</summary>
    [HttpGet("recommended")]
    public async Task<ActionResult<List<MasterListItemResponse>>> GetRecommended([FromQuery] int limit = 10)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Client)
            return Ok(new List<MasterListItemResponse>());

        var result = await _userService.GetRecommendedMasters(userId, Math.Clamp(limit, 1, 20));
        return Ok(result);
    }

    /// <summary>Profil trenutnog majstora (user + kategorija, ocena). Master i CompanyWorker.</summary>
    [HttpGet("profile")]
    public async Task<ActionResult<MasterProfileResponse>> GetMyProfile()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master && role != UserRole.CompanyWorker)
            return Forbid();

        var user = await _userService.GetProfile(userId);
        if (user == null)
            return NotFound();

        Guid? employerCompanyId = null;
        string? employerCompanyName = null;
        if (role == UserRole.CompanyWorker)
        {
            var domainUser = await _userService.GetById(userId);
            var cid = domainUser?.EmployerCompanyId;
            if (cid is { } id && id != Guid.Empty)
            {
                employerCompanyId = id;
                var company = await _companyRepository.GetById(id);
                employerCompanyName = company?.Name;
            }
        }

        var master = await _masterRepository.GetByUserId(userId);
        var response = new MasterProfileResponse
        {
            User = user,
            Category = master?.ServiceCategories?.FirstOrDefault(),
            Rating = master?.AverageRating?.Value,
            EmployerCompanyId = employerCompanyId,
            EmployerCompanyName = employerCompanyName,
            YearsOfExperience = master?.YearsOfExperience ?? 0,
            HourlyRateAmount = master?.HourlyRate?.Amount ?? 0,
            HourlyRateCurrency = master?.HourlyRate?.Currency ?? "RSD",
            TotalReviews = master?.TotalReviews ?? 0
        };
        return Ok(response);
    }

    /// <summary>Recenzije gde je trenutni korisnik ocenjen kao majstor (Master / CompanyWorker).</summary>
    [HttpGet("profile/reviews")]
    public async Task<ActionResult<List<MasterReviewListItemResponse>>> GetMyReviews()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master && role != UserRole.CompanyWorker)
            return Forbid();

        var reviews = await _reviewRepository.GetByMasterId(userId);
        var result = new List<MasterReviewListItemResponse>(reviews.Count);

        foreach (var r in reviews)
        {
            var reviewer = await _userService.GetById(r.ReviewerUserId);
            var name = reviewer != null
                ? $"{reviewer.FirstName} {reviewer.LastName}".Trim()
                : string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                name = "Korisnik";

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

        return Ok(result);
    }

    [HttpPatch("profile/stats")]
    public async Task<IActionResult> PatchProfileStats([FromBody] UpdateMasterProfileStatsRequest body)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master && role != UserRole.CompanyWorker)
            return Forbid();

        if (body is null ||
            (!body.YearsOfExperience.HasValue && !body.HourlyRateAmount.HasValue))
            return BadRequest("Pošalji bar jedno polje za izmenu.");

        var master = await _masterRepository.GetByUserId(userId);
        if (master is null)
            return NotFound();

        if (body.YearsOfExperience.HasValue)
            master.UpdateYearsOfExperience(body.YearsOfExperience.Value);

        if (body.HourlyRateAmount.HasValue)
        {
            var cur = string.IsNullOrWhiteSpace(body.HourlyRateCurrency)
                ? "RSD"
                : body.HourlyRateCurrency.Trim().ToUpperInvariant();
            master.SetHourlyRate(new Money(body.HourlyRateAmount.Value, cur));
        }

        await _masterRepository.Save(userId, master);
        var categoryDisplayName = master.ServiceCategories.FirstOrDefault();
        await _userGraphSync.SyncMasterProfile(
            userId,
            categoryDisplayName,
            master.AverageRating?.Value,
            master.YearsOfExperience);
        return NoContent();
    }

    [HttpPatch("category")]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateMasterCategoryRequest request)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master && role != UserRole.CompanyWorker)
            return Forbid();

        var master = await _masterRepository.GetByUserId(userId);
        if (master == null)
            return NotFound();

        var categoryDisplayName = string.IsNullOrWhiteSpace(request?.Category) ? "Ostalo" : request.Category!.Trim();
        master.ReplaceServiceCategories(new[] { categoryDisplayName });
        await _masterRepository.Save(userId, master);
        await _userGraphSync.SyncMasterProfile(userId, categoryDisplayName, master.AverageRating?.Value, master.YearsOfExperience);
        return NoContent();
    }

    private static List<int> ParseIntList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<int>();
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToList();
    }

    private static List<string> ParseStringList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<string>();
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }
}
