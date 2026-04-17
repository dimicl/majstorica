using backend.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace backend.Api.Hubs;

public class SignalRCompanyInvitationSender : ICompanyInvitationRealtimeSender
{
    private readonly IHubContext<DocumentHub> _hubContext;

    public SignalRCompanyInvitationSender(IHubContext<DocumentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendInvitationAsync(
        Guid masterUserId,
        Guid invitationId,
        Guid companyId,
        string companyName) =>
        _hubContext.Clients
            .User(masterUserId.ToString())
            .SendAsync(
                "CompanyInvitation",
                new
                {
                    invitationId,
                    companyId,
                    companyName,
                    createdAtUtc = DateTime.UtcNow
                });
}
