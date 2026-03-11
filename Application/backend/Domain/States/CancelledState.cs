using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public class CancelledState : IJobState
{
    public JobStatus Status => JobStatus.Cancelled;

    public void CanPublish()
    {
        throw new InvalidJobStateException("Cancelled job cannot be published.");
    }

    public void CanAssign()
    {
        throw new InvalidJobStateException("Cancelled job cannot be assigned.");
    }

    public void CanStart()
    {
        throw new InvalidJobStateException("Cancelled job cannot be started.");
    }

    public void CanComplete()
    {
        throw new InvalidJobStateException("Cancelled job cannot be completed.");
    }

    public void CanCancel()
    {
        throw new InvalidJobStateException("Cancelled job cannot be cancelled again.");
    }

    public void CanExpire()
    {
        throw new InvalidJobStateException("Cancelled job cannot expire.");
    }
}