using backend.Application.Messages;

namespace backend.Application.Interfaces;

public interface IJobRequestRealtimeSender
{
    Task SendNewJobRequestAsync(Guid masterId, NewJobRequestMessage message);
}
