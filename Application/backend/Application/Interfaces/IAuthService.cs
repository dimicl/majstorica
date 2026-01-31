using backend.Api.DTOs.Auth;
using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> Register(
        string firstName,
        string lastName,
        string email,
        string username,
        string password,
        UserRole role);

    Task<AuthResponse> Login(string usernameOrEmail, string password);
}
