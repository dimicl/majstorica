using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IClientRepository
{
    Task Save(Client client);
    Task<Client?> GetById(Guid id);
    Task<Client?> GetByUserId(Guid userId);
}
