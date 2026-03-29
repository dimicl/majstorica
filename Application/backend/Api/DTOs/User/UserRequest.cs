using backend.Domain.Enums;

namespace backend.Api.DTOs.User;

public class UserRequest
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public UserRole Role { get; set; }
    public string? Phone { get; set; }
    public AddressResponse? Address { get; set; }
}