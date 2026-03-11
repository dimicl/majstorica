using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public class AssignedState : IJobState
{
    public JobStatus Status => JobStatus.Assigned;

    public void CanPublish()
    {
        throw new InvalidJobStateException("Assigned job cannot be published.");
    }

    public void CanAssign()
    {
        throw new InvalidJobStateException("Assigned job cannot be assigned again.");
    }

    public void CanStart()
    {
        // dozvoljeno
    }

    public void CanComplete()
    {
        throw new InvalidJobStateException("Assigned job cannot be completed before start.");
    }

    public void CanCancel()
    {
        // dozvoljeno
    }

    public void CanExpire()
    {
        throw new InvalidJobStateException("Assigned job cannot expire.");
    }
}