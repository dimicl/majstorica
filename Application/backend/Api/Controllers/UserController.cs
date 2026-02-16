using backend.Application.Interfaces;
using backend.Api.DTOs.User;
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
    public async Task<ActionResult<UserRequest>> GetById(Guid id)
    {
        var _user = await _userService.GetById(id);
        if (_user == null) return NotFound();
    
        var user = new UserRequest
        {
            Id = _user.Id,
            Username = _user.Username,
            FirstName = _user.FirstName,
            LastName = _user.LastName,
            Role = _user.Role
        };

        return Ok(new { user });
    }
}

