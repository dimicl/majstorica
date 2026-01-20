using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Domain.States;

public static class JobStateFactory
{
    public static IJobState Create(JobStatus status, Job job)
    {
        return status switch
        {
            JobStatus.Created => new CreatedState(job),
            JobStatus.InProgress => new InProgressState(job),
            JobStatus.Completed => new CompletedState(job),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                $"Nepodržano stanje posla: {status}")
        };
    }
}
