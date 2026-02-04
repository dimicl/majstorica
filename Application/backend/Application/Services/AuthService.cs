using backend.Api.DTOs.Auth;
using backend.Application.Interfaces;
using backend.Shared.Helpers;
using backend.Shared.Exceptions;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUserGraphSync _userGraphSync;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository users,
        IUserGraphSync userGraphSync,
        IConfiguration config)
    {
        _users = users;
        _userGraphSync = userGraphSync;
        _config = config;
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        var user = await AuthHelper.CreateUserAndSync(
            _users,
            _userGraphSync,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Username,
            request.Password,
            request.Role,
            request.Phone,
            request.DeliveryAddress);

        return AuthHelper.BuildAuthResponse(user, _config);
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var user =
            await _users.GetByEmail(request.UsernameOrEmail)
            ?? await _users.GetByUsername(request.UsernameOrEmail);

        if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return AuthHelper.BuildAuthResponse(user, _config);
    }
}
