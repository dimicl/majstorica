using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Helpers;
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

    public async Task<string> Register(
        string firstName,
        string lastName,
        string email,
        string username,
        string password,
        UserRole role)
    {
        if (await _users.GetByEmail(email) != null)
            throw new Exception("Email je već zauzet.");
        if (await _users.GetByUsername(username) != null)
            throw new Exception("Username je već zauzet.");

        var user = new User(
            firstName,
            lastName,
            email,
            username,
            password,
            role);

        await _users.Save(user);

        return JwtHelper.Generate(user, _config);
    }

    public async Task<string> Login(string usernameOrEmail, string password)
    {
        var user =
            await _users.GetByEmail(usernameOrEmail)
            ?? await _users.GetByUsername(usernameOrEmail);

        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new Exception("Pogrešni kredencijali.");

        return JwtHelper.Generate(user, _config);
    }
}
