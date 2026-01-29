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
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request)
    {
        var token = await _authService.Register(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Username,
            request.Password,
            request.Role
        );

        return Ok(new AuthResponse
        {
            Token = token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var token = await _authService.Login(
            request.UsernameOrEmail,
            request.Password
        );

        return Ok(new AuthResponse
        {
            Token = token
        });
    }
}
