using backend.Domain.Enums;
using backend.Shared.Helpers;
using Neo4j.Driver;

namespace backend.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Username { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public UserRole Role { get; private set; }
    //za soft delete, da ga ne brisemo iz baze jer ostaju viseci poslovi, puca statistika..., da ga deaktiviramo i da ne moze da se uloguje
    public bool IsActive { get; private set; }

    protected User() { }

    public User(
        string firstName,
        string lastName,
        string email,
        string username,
        string plainPassword,
        UserRole role)
    {
        Id = Guid.NewGuid();

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Username = username;

        PasswordHash = PasswordHasher.Hash(plainPassword);

        Role = role;
        IsActive = true;
    }

    // ---------------- DOMENSKE OPERACIJE ----------------

    public void ChangePassword(string oldPassword, string newPassword)
    {
        if (!PasswordHasher.Verify(oldPassword, PasswordHash))
            throw new Exception("Pogrešna lozinka.");

        PasswordHash = PasswordHasher.Hash(newPassword);
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public static User Rehydrate(INode node)
    {
        var user = new User();

        user.Id = Guid.Parse(node.Properties["id"].As<string>());
        user.FirstName = node.Properties["firstName"].As<string>();
        user.LastName = node.Properties["lastName"].As<string>();
        user.Email = node.Properties["email"].As<string>();
        user.Username = node.Properties["username"].As<string>();
        user.PasswordHash = node.Properties["passwordHash"].As<string>();
        user.Role = Enum.Parse<UserRole>(node.Properties["role"].As<string>());
        user.IsActive = node.Properties["isActive"].As<bool>();

        return user;
    }
}
