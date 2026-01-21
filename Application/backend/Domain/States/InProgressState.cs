using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class InProgressState : IJobState
{
    private readonly Job _job;

    public InProgressState(Job job)
    {
        _job = job;
    }
    
    public void AssignMaster(Job job, Guid masterId)
    {
        throw new DomainException(
            "Majstor je već dodeljen. Ne možete menjati majstora dok je posao u toku.");
    }

    public void ChangeDescription(Job job, string description)
    {
        throw new DomainException(
            "Opis posla se ne može menjati dok je posao u toku.");
    }

    public void Start(Job job)
    {
        throw new DomainException(
            "Posao je već započet.");
    }

    public void Complete(Job job)
    {
        job.SetStatus(JobStatus.Completed);
    }
}
