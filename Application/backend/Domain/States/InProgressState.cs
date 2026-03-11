using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public class InProgressState : IJobState
{
    public JobStatus Status => JobStatus.InProgress;

    public void CanPublish()
    {
        throw new InvalidJobStateException("In-progress job cannot be published.");
    }

    public void CanAssign()
    {
        throw new InvalidJobStateException("In-progress job cannot be assigned.");
    }

    public void CanStart()
    {
        throw new InvalidJobStateException("In-progress job cannot be started again.");
    }

    public void CanComplete()
    {
        // dozvoljeno
    }

    public void CanCancel()
    {
        // dozvoljeno
    }

    public void CanExpire()
    {
        throw new InvalidJobStateException("In-progress job cannot expire.");
    }
}