using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class UserMapper
{
    /// <summary>
    /// Normalizes role loaded from DB (e.g. old enum value 0 → Client) so User constructor validation does not throw.
    /// </summary>
    private static UserRole NormalizeStoredRole(UserRole role)
    {
        if ((int)role == 0 || !Enum.IsDefined(typeof(UserRole), role))
            return UserRole.Client;
        return role;
    }
    public static UserDocument ToDocument(User user)
    {
        var masterProfile = user.MasterProfile != null ? MasterMapper.ToDocument(user.MasterProfile) : null;
        return new UserDocument
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address?.ToString(),
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc,
            MasterProfile = masterProfile
        };
    }

    public static User ToDomain(UserDocument doc)
    {
        var role = NormalizeStoredRole(doc.Role);
        // Stari dokumenti mogu imati null/prazan PhoneNumber – domen zahteva ne-prazan string
        var phone = string.IsNullOrWhiteSpace(doc.PhoneNumber) ? "Nepoznat" : doc.PhoneNumber;
        var user = new User(
            doc.Id,
            doc.FirstName,
            doc.LastName,
            doc.Email,
            doc.Username,
            phone,
            null,
            doc.PasswordHash,
            role,
            doc.CreatedAtUtc);

        if (doc.MasterProfile != null)
        {
            var profile = MasterMapper.ToDomain(doc.MasterProfile);
            user.SetMasterProfile(profile);
        }

        return user;
    }
}
