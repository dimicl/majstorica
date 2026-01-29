using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class AcceptedState : IJobState
{
    public AcceptedState() { }

    public void SendRequests(Job job)
        => throw new DomainException("Posao je već prihvaćen od majstora.");

    public void Accept(Job job, Guid masterId)
        => throw new DomainException("Posao je već prihvaćen.");

    public void Start(Job job)
    {
        if (job.MasterId == null)
            throw new DomainException("Nema dodeljenog majstora.");

        job.SetStatus(JobStatus.InProgress);
    }

    public void Complete(Job job)
        => throw new DomainException("Ne možete završiti posao koji nije započet.");

    public void ChangeDescription(Job job, string description)
        => job.ChangeDescriptionInternal(description);

    public void ChangePrice(Job job, decimal? price)
        => job.ChangePriceInternal(price);
}
