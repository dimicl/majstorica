namespace backend.Domain.Events;

// Omotac za domain event u RabbitMQ poruci – omogućava consumeru da deserijalizuje na pravi tip.
public class DomainEventEnvelope
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}
