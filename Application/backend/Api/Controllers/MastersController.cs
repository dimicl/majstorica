using backend.Api.DTOs.Master;
using backend.Api.Extensions;
using backend.Application.Interfaces;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/masters")]
[Authorize]
public class MastersController : ControllerBase
{
    private readonly IUserService _userService;

    public MastersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<MastersListPageResponse>> GetMasters([FromQuery] MastersListQuery? query = null)
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

        var response = await _userService.GetMasterProfile(userId);
        if (response == null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>Recenzije gde je trenutni korisnik ocenjen kao majstor (Master / CompanyWorker).</summary>
    [HttpGet("profile/reviews")]
    public async Task<ActionResult<List<MasterReviewListItemResponse>>> GetMyReviews()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master && role != UserRole.CompanyWorker)
            return Forbid();

        var result = await _userService.GetMasterReviews(userId);
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

        await _userService.UpdateMasterProfileStats(userId, body.YearsOfExperience, body.HourlyRateAmount, body.HourlyRateCurrency);
        return NoContent();
    }

    [HttpPatch("category")]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateMasterCategoryRequest request)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master && role != UserRole.CompanyWorker)
            return Forbid();

        await _userService.UpdateMasterCategory(userId, request?.Category);
        return NoContent();
    }

    private static List<string> ParseStringList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<string>();
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }
}