namespace backend.Application.Interfaces;

public interface ICompanyInvitationRealtimeSender
{
    Task SendInvitationAsync(
        Guid masterUserId,
        Guid invitationId,
        Guid companyId,
        string companyName);
}
