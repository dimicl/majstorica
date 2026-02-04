using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class UserMapper
{
    public static UserDocument ToDocument(User user)
    {
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
            Phone = user.Phone,
            DeliveryAddress = user.DeliveryAddress
        };
    }

    public static User ToDomain(UserDocument doc)
    {
        return User.Rehydrate(
            doc.Id,
            doc.FirstName,
            doc.LastName,
            doc.Email,
            doc.Username,
            doc.PasswordHash,
            doc.Role,
            doc.IsActive,
            doc.Phone,
            doc.DeliveryAddress);
    }
}
