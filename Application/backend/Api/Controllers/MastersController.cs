using backend.Api.DTOs.Master;
using backend.Application.Interfaces;
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
    public async Task<ActionResult<List<MasterListItemResponse>>> GetMasters()
    {
        var masters = await _userService.GetAllMasters();
        var result = masters.Select(u => new MasterListItemResponse
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Username = u.Username
        }).ToList();
        return Ok(result);
    }
}
