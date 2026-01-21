using backend.Domain.Entities;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class CompletedState : IJobState
{
    private readonly Job _job;

    public CompletedState(Job job)
    {
        _job = job;
    }
    
    public void AssignMaster(Job job, Guid masterId)
    {
        throw new DomainException(
            "Završeni posao ne može da se menja.");
    }

    public void ChangeDescription(Job job, string description)
    {
        throw new DomainException(
            "Završeni posao se ne može menjati.");
    }

    public void Start(Job job)
    {
        throw new DomainException(
            "Završeni posao se ne može ponovo pokrenuti.");
    }

    public void Complete(Job job)
    {
        throw new DomainException(
            "Posao je već završen.");
    }
}
