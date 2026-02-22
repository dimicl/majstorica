using backend.Application.Interfaces;
using backend.Application.Messages;

namespace backend.Application.Services;

public class JobRequestNotifier : IJobRequestNotifier
{
    private readonly IJobRequestRealtimeSender _sender;

    public JobRequestNotifier(IJobRequestRealtimeSender sender)
    {
        _sender = sender;
    }

    public async Task NotifyNewRequest(
        Guid masterId,
        Guid jobId,
        Guid conversationId,
        string jobTitle,
        string description,
        DateTime date,
        string clientName,
        Guid clientId,
        decimal? price,
        bool isEmergency)
    {
        var message = new NewJobRequestMessage(
            jobId,
            conversationId,
            jobTitle,
            description,
            date,
            clientName,
            clientId,
            price,
            isEmergency);

        await _sender.SendNewJobRequestAsync(masterId, message);
    }
}
