using backend.Domain.Entities;
using backend.Shared.Exceptions;

namespace backend.Domain.States;

public class CompletedState : IJobState
{
    public CompletedState() { }

    public void SendRequests(Job job) => throw new DomainException("Završeni posao se ne menja.");
    public void Accept(Job job, Guid masterId) => throw new DomainException("Završeni posao se ne menja.");
    public void Start(Job job) => throw new DomainException("Završeni posao se ne može pokrenuti.");
    public void Complete(Job job) => throw new DomainException("Posao je već završen.");

    public void ChangeDescription(Job job, string description)
        => throw new DomainException("Završeni posao se ne može menjati.");

    public void ChangePrice(Job job, decimal? price)
        => throw new DomainException("Završeni posao se ne može menjati.");
}
