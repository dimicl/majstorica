namespace backend.Api.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public UserRequest User { get; set; } = default!;
}
