using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetById(Guid id);
    Task<User?> GetByEmail(string email);
    Task<User?> GetByUsername(string username);

    Task<List<User>> GetAll();
    Task<List<User>> GetByIds(IEnumerable<Guid> ids);

    Task Save(User user);
}
