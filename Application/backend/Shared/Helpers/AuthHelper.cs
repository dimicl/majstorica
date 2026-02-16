using backend.Api.DTOs.Auth;
using backend.Api.DTOs.User;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Shared.Helpers;

internal static class AuthHelper
{
    public static async Task<User> CreateUserAndSync(
        IUserRepository users,
        IUserGraphSync userGraphSync,
        string firstName,
        string lastName,
        string email,
        string username,
        string password,
        UserRole role,
        string? phone,
        string? deliveryAddress)
    {
        if (await users.GetByEmail(email) != null)
            throw new UserAlreadyExistsException("Email", email);

        if (await users.GetByUsername(username) != null)
            throw new UserAlreadyExistsException("Username", username);

        var user = new User(firstName, lastName, email, username, password, role, phone, deliveryAddress);
        await users.Save(user);
        await userGraphSync.SyncUserNode(user.Id, user.Role);
        return user;
    }

    public static AuthResponse BuildAuthResponse(User user, IConfiguration config)
    {
        var token = JwtHelper.Generate(user, config);
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
