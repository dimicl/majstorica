using backend.Domain.Enums;
using backend.Shared.Exceptions;
using backend.Shared.Helpers;

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

    public bool IsActive { get; private set; }

    public string? Phone { get; private set; }
    public string? DeliveryAddress { get; private set; }

    protected User() { }

    public User(
        string firstName,
        string lastName,
        string email,
        string username,
        string plainPassword,
        UserRole role,
        string? phone = null,
        string? deliveryAddress = null)
    {
        Id = Guid.NewGuid();

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Username = username;

        PasswordHash = PasswordHasher.Hash(plainPassword);

        Role = role;
        IsActive = true;
        Phone = phone;
        DeliveryAddress = deliveryAddress;
    }

    // ---------------- DOMENSKE OPERACIJE ----------------

    public void ChangePassword(string oldPassword, string newPassword)
    {
        if (!PasswordHasher.Verify(oldPassword, PasswordHash))
            throw new InvalidCredentialsException("Pogrešna lozinka.");

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

    public void UpdateContact(string? phone, string? deliveryAddress)
    {
        Phone = phone;
        DeliveryAddress = deliveryAddress;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public static User Rehydrate(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string username,
        string passwordHash,
        UserRole role,
        bool isActive,
        string? phone = null,
        string? deliveryAddress = null)
    {
        return new User
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = isActive,
            Phone = phone,
            DeliveryAddress = deliveryAddress
        };
    }
}
