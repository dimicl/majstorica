using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.States;

public class ExpiredState : IJobState
{
    public JobStatus Status => JobStatus.Expired;

    public void CanPublish()
    {
        throw new InvalidJobStateException("Expired job cannot be published.");
    }

    public void CanAssign()
    {
        throw new InvalidJobStateException("Expired job cannot be assigned.");
    }

    public void CanStart()
    {
        throw new InvalidJobStateException("Expired job cannot be started.");
    }

    public void CanComplete()
    {
        throw new InvalidJobStateException("Expired job cannot be completed.");
    }

    public void CanCancel()
    {
        throw new InvalidJobStateException("Expired job cannot be cancelled.");
    }

    public void CanExpire()
    {
        throw new InvalidJobStateException("Expired job cannot expire again.");
    }
}