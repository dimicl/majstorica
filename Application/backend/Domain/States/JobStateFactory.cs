using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public static class JobStateFactory
{
    public static IJobState Create(JobStatus status)
    {
        return status switch
        {
            JobStatus.Draft => new DraftState(),
            JobStatus.Published => new PublishedState(),
            JobStatus.Assigned => new AssignedState(),
            JobStatus.InProgress => new InProgressState(),
            JobStatus.Completed => new CompletedState(),
            JobStatus.Cancelled => new CancelledState(),
            JobStatus.Expired => new ExpiredState(),
            _ => throw new DomainException($"Unsupported job status: {status}")
        };
    }
}