using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public class PublishedState : IJobState
{
    public JobStatus Status => JobStatus.Published;

    public void CanPublish()
    {
        throw new InvalidJobStateException("Published job cannot be published again.");
    }

    public void CanAssign()
    {
        // dozvoljeno
    }

    public void CanStart()
    {
        throw new InvalidJobStateException("Published job cannot be started before assignment.");
    }

    public void CanComplete()
    {
        throw new InvalidJobStateException("Published job cannot be completed.");
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