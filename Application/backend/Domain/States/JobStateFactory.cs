using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Domain.States;

//da spojimo state klase sa poslovima
public static class JobStateFactory
{
    public static IJobState Create(JobStatus status) => status switch
    {
        JobStatus.Created => new CreatedState(),
        JobStatus.Pending => new PendingState(),
        JobStatus.Accepted => new AcceptedState(),
        JobStatus.InProgress => new InProgressState(),
        JobStatus.Completed => new CompletedState(),
        _ => throw new ArgumentOutOfRangeException(nameof(status), $"Nepodržano stanje posla: {status}")

        //TREBA DA SE DODA CANCELLEDSTATE
    };
}
