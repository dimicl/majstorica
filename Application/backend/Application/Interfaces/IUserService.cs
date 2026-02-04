using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IUserService
{
    Task<User?> GetById(Guid userId);

    Task UpdateProfile(
        Guid userId,
        string firstName,
        string lastName);

    Task UpdateContact(Guid userId, string? phone, string? deliveryAddress);


    Task Deactivate(Guid userId);
    Task Activate(Guid userId);

    Task<List<User>> GetAllMasters();
}
