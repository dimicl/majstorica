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
        var result = await _userService.GetMastersList();
        return Ok(result);
    }
}
