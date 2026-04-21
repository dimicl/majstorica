using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface ICompanyInvitationRepository
{
    Task<CompanyInvitation?> GetById(Guid id);

    Task<CompanyInvitation?> GetPendingByCompanyAndMaster(Guid companyId, Guid masterUserId);

    Task<List<CompanyInvitation>> GetPendingForMaster(Guid masterUserId);

    /// <summary>Pending pozivnice koje je ova firma poslala majstorima.</summary>
    Task<List<CompanyInvitation>> GetPendingForCompany(Guid companyId);

    Task Save(CompanyInvitation invitation);
}
