using backend.Application.Interfaces;
using backend.Application.Messages;
using Microsoft.AspNetCore.SignalR;

namespace backend.Api.Hubs;

public class SignalRJobRequestSender : IJobRequestRealtimeSender
{
    private readonly IHubContext<DocumentHub> _hubContext;

    public SignalRJobRequestSender(IHubContext<DocumentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNewJobRequestAsync(Guid masterId, NewJobRequestMessage message)
    {
        await _hubContext.Clients
            .User(masterId.ToString())
            .SendAsync("NewJobRequest", new
            {
                message.JobId,
                message.ConversationId,
                message.JobTitle,
                message.Description,
                message.Date,
                message.ClientName,
                message.ClientId,
                message.Price,
                message.IsEmergency
            });
    }
}
