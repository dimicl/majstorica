using backend.Api.DTOs.Auth;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Helpers;
using backend.Shared.Exceptions;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<AuthResponse> Register(
        string firstName,
        string lastName,
        string email,
        string username,
        string password,
        UserRole role)
    {
        if (await _users.GetByEmail(email) != null)
            throw new UserAlreadyExistsException("Email", email);
        
        if (await _users.GetByUsername(username) != null)
            throw new UserAlreadyExistsException("Username", username);

        var user = new User(
            firstName,
            lastName,
            email,
            username,
            password,
            role);

        await _users.Save(user);

        var token = JwtHelper.Generate(user, _config);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserRequest
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponse> Login(string usernameOrEmail, string password)
    {
        var user =
            await _users.GetByEmail(usernameOrEmail)
            ?? await _users.GetByUsername(usernameOrEmail);

        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new InvalidCredentialsException();

        var token = JwtHelper.Generate(user, _config);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserRequest
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            }
        };
    }
}
