namespace backend.Domain.Events;

/// <summary>
/// Omotac za domain event u RabbitMQ poruci – omogućava consumeru da deserijalizuje na pravi tip.
/// </summary>
public class DomainEventEnvelope
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
