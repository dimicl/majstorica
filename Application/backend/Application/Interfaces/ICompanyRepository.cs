using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetById(Guid id);
    Task<Company?> GetByOwnerUserId(Guid ownerUserId);
    Task Save(Company company);
}
