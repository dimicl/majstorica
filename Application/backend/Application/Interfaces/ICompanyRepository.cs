using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetById(Guid id);
    Task<Company?> GetByOwnerUserId(Guid ownerUserId);
    Task<IReadOnlyList<Company>> GetAllActive();
    Task Save(Company company);
}
