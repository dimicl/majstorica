using backend.Api.DTOs.Company;

namespace backend.Application.Interfaces;

public interface ICompanyService
{
    Task<CompanyResponse?> GetMineForOwner(Guid ownerUserId);

    Task<CompanyPublicResponse?> GetPublicById(Guid companyId);
    Task<CompanyResponse> CreateForOwner(
        Guid ownerUserId,
        string name,
        string phoneNumber,
        string email,
        string? street,
        string? city);

    Task<List<MasterSearchForInviteResponse>> SearchMastersForInvite(
        Guid ownerUserId,
        string? query,
        int limit);

    Task InviteMaster(Guid ownerUserId, Guid masterUserId);

    /// <summary>ID-jevi majstora kojima firma vlasnika još čeka na odgovor na poziv.</summary>
    Task<List<Guid>> GetPendingOutboundInviteMasterIdsForOwner(Guid ownerUserId);

    Task<List<CompanyInvitationPendingResponse>> GetPendingInvitationsForMaster(Guid masterUserId);

    Task AcceptInvitation(Guid masterUserId, Guid invitationId);

    Task DeclineInvitation(Guid masterUserId, Guid invitationId);

    /// <summary>Majstori koji su prihvatili poziv (CompanyWorker u ovoj firmi).</summary>
    Task<List<CompanyWorkerMemberResponse>> GetWorkersForCompanyOwner(Guid ownerUserId);
}
