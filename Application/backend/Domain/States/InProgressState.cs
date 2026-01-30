using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class InProgressState : IJobState
{
    public InProgressState() { }

    public void SendRequests(Job job) => throw new DomainException("Posao je već u toku.");
    public void Accept(Job job, Guid masterId) => throw new DomainException("Posao je već prihvaćen/započet.");
    public void Start(Job job) => throw new DomainException("Posao je već započet.");

    public void Complete(Job job)
        => job.SetStatus(JobStatus.Completed);

    public void ChangeDescription(Job job, string description)
        => throw new DomainException("Opis se ne može menjati dok je posao u toku.");

    public void ChangePrice(Job job, decimal? price)
        => throw new DomainException("Cena se ne može menjati dok je posao u toku.");
}
