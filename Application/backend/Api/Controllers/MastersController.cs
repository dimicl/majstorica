using backend.Api.DTOs.Master;
using backend.Api.Extensions;
using backend.Application.Interfaces;
using backend.Domain.Entities;
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
    private readonly IMasterRepository _masterRepository;
    private readonly IUserGraphSync _userGraphSync;

    public MastersController(
        IUserService userService,
        IMasterRepository masterRepository,
        IUserGraphSync userGraphSync)
    {
        _userService = userService;
        _masterRepository = masterRepository;
        _userGraphSync = userGraphSync;
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
        var categoryIds = ParseIntList(query.CategoryIds);
        var zoneIds = ParseStringList(query.ZoneIds);
        var limit = Math.Clamp(query.Limit, 1, 50);
        var result = await _userService.GetMastersByGraphSearch(
            categoryIds.Count > 0 ? categoryIds : null,
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

    /// <summary>Profil trenutnog majstora (user + kategorija, ocena). Samo za ulogu Master.</summary>
    [HttpGet("profile")]
    public async Task<ActionResult<MasterProfileResponse>> GetMyProfile()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master)
            return Forbid();

        var user = await _userService.GetProfile(userId);
        if (user == null)
            return NotFound();

        var master = await _masterRepository.GetByUserId(userId);
        var response = new MasterProfileResponse
        {
            User = user,
            Category = master?.Category.HasValue == true
                ? MasterCategoryDisplay.ToDisplayName(master.Category!.Value)
                : null,
            Rating = master?.Rating
        };
        return Ok(response);
    }

    /// <summary>Ažurira kategoriju majstora (npr. "Električar"). Prazan string ili null uklanja kategoriju. Ako Master dokument ne postoji, kreira se.</summary>
    [HttpPatch("category")]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateMasterCategoryRequest request)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master)
            return Forbid();

        var category = MasterCategoryDisplay.FromDisplayName(request?.Category);
        var master = await _masterRepository.GetByUserId(userId);
        if (master == null)
        {
            master = new Master(userId, null, category, null);
            await _masterRepository.Save(master);
        }
        else
        {
            master.UpdateCategory(category);
            await _masterRepository.Save(master);
        }

        await _userGraphSync.SyncMasterProfile(userId, master.Category, master.Rating, master.YearsExperience);
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
