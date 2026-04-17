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
    private readonly IAccountActivityStore _activity;
    private readonly IAuthServerSessionStore _authServerSession;

    public AuthService(
        IUserRepository users,
        IUserGraphSync userGraphSync,
        IConfiguration config,
        IAccountActivityStore activity,
        IAuthServerSessionStore authServerSession)
    {
        _users = users;
        _userGraphSync = userGraphSync;
        _config = config;
        _activity = activity;
        _authServerSession = authServerSession;
    }

    private async Task TouchAuthSidecarAsync(Guid userId, string activityType)
    {
        try
        {
            await _authServerSession.TouchServerSessionAsync(userId);
            await _activity.RecordAsync(userId, activityType);
        }
        catch
        {
            // Redis nedostupan — autentikacija i dalje prolazi
        }
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

        var response = AuthHelper.BuildAuthResponse(user, _config);
        await TouchAuthSidecarAsync(user.Id, "register");
        return response;
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

        var response = AuthHelper.BuildAuthResponse(user, _config);
        await TouchAuthSidecarAsync(user.Id, "login");
        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(Guid userId)
    {
        var user = await _users.GetById(userId)
            ?? throw new NotFoundException("Korisnik nije pronađen.");
        var response = AuthHelper.BuildAuthResponse(user, _config);
        await TouchAuthSidecarAsync(user.Id, "token_refresh");
        return response;
    }
}
