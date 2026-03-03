using backend.Application.Interfaces;
using backend.Api.DTOs.User;
using backend.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetById(Guid id)
    {
        var user = await _userService.GetProfile(id);
        if (user == null) return NotFound();
        return Ok(new { user });
    }

    /// <summary>Postavlja zonu trenutnog korisnika u Neo4j (za pretragu majstora po lokaciji).</summary>
    [HttpPatch("zone")]
    public async Task<IActionResult> SetZone([FromBody] SetUserZoneRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ZoneId) || string.IsNullOrWhiteSpace(request.ZoneName))
            return BadRequest("ZoneId i ZoneName su obavezni.");
        var userId = User.GetUserId();
        await _userService.SetUserZone(userId, request.ZoneId, request.ZoneName);
        return NoContent();
    }
}

