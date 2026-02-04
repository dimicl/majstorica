using backend.Api.DTOs.Auth;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (request.Role == Domain.Enums.UserRole.Admin)
            return BadRequest(new { message = "Ne možete se registrovati kao Admin." });

        var response = await _authService.Register(request);
        return StatusCode(201, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var response = await _authService.Login(request);

        return Ok(response);
    }
}
