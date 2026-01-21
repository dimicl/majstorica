using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class CreatedState : IJobState
{
    private readonly Job _job;

    public CreatedState(Job job)
    {
        _job = job;
    }
    
    public void AssignMaster(Job job, Guid masterId)
    {
        job.SetMaster(masterId);
        job.SetStatus(JobStatus.InProgress);
    }

    public void ChangeDescription(Job job, string description)
    {
        job.ChangeDescriptionInternal(description);
    }

    public void Start(Job job)
    {
        throw new DomainException(
            "Ne možete započeti posao bez dodeljenog majstora.");
    }

    public void Complete(Job job)
    {
        throw new DomainException(
            "Ne možete završiti posao koji nije započet.");
    }
}
