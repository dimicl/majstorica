using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class PendingState : IJobState
{
    public PendingState() { }

    public void SendRequests(Job job)
        => throw new DomainException("Zahtevi su već poslati.");

    public void Accept(Job job, Guid masterId)
    {
        job.SetMaster(masterId);
        job.SetStatus(JobStatus.Accepted);
    }

    public void Start(Job job)
        => throw new DomainException("Ne možete započeti posao pre prihvatanja.");

    public void Complete(Job job)
        => throw new DomainException("Ne možete završiti posao koji nije započet.");

    public void ChangeDescription(Job job, string description)
        => job.ChangeDescriptionInternal(description);

    public void ChangePrice(Job job, decimal? price)
        => job.ChangePriceInternal(price);
}
