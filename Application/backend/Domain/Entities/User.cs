using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }

    public string Password { get; private set; }

    public UserRole Role { get; private set; }

    protected User() { }

    public User(
        string firstName,
        string lastName,
        string email,
        string username,
        string password,
        UserRole role)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Username = username;
        Password = password;
        Role = role;
    }
}
