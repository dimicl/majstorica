using backend.Api.DTOs.Auth;
using backend.Application.Interfaces;
using backend.Domain.ValueObjects;
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
        Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.DeliveryAddress) ||
            !string.IsNullOrWhiteSpace(request.City))
        {
            var street = string.IsNullOrWhiteSpace(request.DeliveryAddress)
                ? "Nepoznata adresa"
                : request.DeliveryAddress;
            var city = string.IsNullOrWhiteSpace(request.City)
                ? "Nepoznat grad"
                : request.City;

            address = new Address(street, city);
        }

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
            address);

        return AuthHelper.BuildAuthResponse(user, _config);
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var user =
            await _users.GetByEmail(request.UsernameOrEmail)
            ?? await _users.GetByUsername(request.UsernameOrEmail);

        if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        // Ažuriraj heš ako je lozinka bila sačuvana kao običan tekst (legacy)
        if (user.PasswordHash.IndexOf('.') < 0)
        {
            user.ChangePassword(PasswordHasher.Hash(request.Password));
            await _users.Save(user);
        }

        return AuthHelper.BuildAuthResponse(user, _config);
    }
}
