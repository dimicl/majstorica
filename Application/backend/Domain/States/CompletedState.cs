using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public class CompletedState : IJobState
{
    public JobStatus Status => JobStatus.Completed;

    public void CanPublish()
    {
        throw new InvalidJobStateException("Completed job cannot be published.");
    }

    public void CanAssign()
    {
        throw new InvalidJobStateException("Completed job cannot be assigned.");
    }

    public void CanStart()
    {
        throw new InvalidJobStateException("Completed job cannot be started.");
    }

    public void CanComplete()
    {
        throw new InvalidJobStateException("Completed job cannot be completed again.");
    }

    public void CanCancel()
    {
        throw new InvalidJobStateException("Completed job cannot be cancelled.");
    }

    public void CanExpire()
    {
        throw new InvalidJobStateException("Completed job cannot expire.");
    }
}