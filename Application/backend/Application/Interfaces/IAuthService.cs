using backend.Api.DTOs.Auth;

namespace backend.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> Register(RegisterRequest request);

    Task<AuthResponse> Login(LoginRequest request);

    /// <summary>Novi JWT nakon promene uloge (npr. prihvat poziva u firmu).</summary>
    Task<AuthResponse> RefreshTokenAsync(Guid userId);
}
