using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public class DraftState : IJobState
{
    public JobStatus Status => JobStatus.Draft;

    public void CanPublish()
    {
        // dozvoljeno
    }

    public void CanAssign()
    {
        throw new InvalidJobStateException("Draft job cannot be assigned.");
    }

    public void CanStart()
    {
        throw new InvalidJobStateException("Draft job cannot be started.");
    }

    public void CanComplete()
    {
        throw new InvalidJobStateException("Draft job cannot be completed.");
    }

    public void CanCancel()
    {
        // dozvoljeno
    }

    public void CanExpire()
    {
        // dozvoljeno
    }
}