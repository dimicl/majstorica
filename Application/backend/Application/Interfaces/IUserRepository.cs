using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetById(Guid id);
    Task<User?> GetByEmail(string email);
    Task<User?> GetByUsername(string username);

    Task<List<User>> GetAll();
    Task<List<User>> GetByIds(IEnumerable<Guid> ids);
    
    /// <summary>Aktivni majstori filtrirani na nivou baze — ne učitava sve korisnike.</summary>
    Task<List<User>> GetActiveMasters();
    
    /// <summary>Slobodni majstori (Master, bez firme) za pretragu po imenu/korisničkom imenu.</summary>
    Task<List<User>> SearchMastersForCompanyInvite(string searchText, int limit, Guid excludeUserId);

    /// <summary>Zaposleni majstori (CompanyWorker) za datu firmu.</summary>
    Task<List<User>> GetWorkersForCompany(Guid companyId);

    Task Save(User user);
}
