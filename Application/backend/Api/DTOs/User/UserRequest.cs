using backend.Domain.Enums;

public class UserRequest
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;
    public UserRole Role { get; set; }
}