namespace backend.Domain.Events;

public abstract class DomainEvent
{
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public DateTime OccurredAtUtc { get; }
}