using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class CreatedState : IJobState
{
    public CreatedState() { }

    public void SendRequests(Job job)
        => job.SetStatus(JobStatus.Pending);

    public void Accept(Job job, Guid masterId)
        => throw new DomainException("Ne možete prihvatiti posao pre slanja zahteva.");

    public void Start(Job job)
        => throw new DomainException("Ne možete započeti posao pre prihvatanja.");

    public void Complete(Job job)
        => throw new DomainException("Ne možete završiti posao koji nije započet.");

    public void ChangeDescription(Job job, string description)
        => job.ChangeDescriptionInternal(description);

    public void ChangePrice(Job job, decimal? price)
        => job.ChangePriceInternal(price);
}
