using backend.Api.DTOs.Auth;
using backend.Api.DTOs.User;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
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
        Address? deliveryAddress)
    {
        if (await users.GetByEmail(email) != null)
            throw new UserAlreadyExistsException("Email", email);

        if (await users.GetByUsername(username) != null)
            throw new UserAlreadyExistsException("Username", username);

        var passwordHash = PasswordHasher.Hash(password);
        var user = new User(Guid.NewGuid(), firstName, lastName, email, username, phone ?? string.Empty, deliveryAddress, passwordHash, role, DateTime.UtcNow);
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
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Phone = user.PhoneNumber,
                DeliveryAddress = user.Address?.ToString()
            }
        };
    }
}
