using backend.Application.Interfaces;
using backend.Domain.Events;

namespace backend.Infrastructure.Messaging;

public class DummyMessagePublisher : IMessagePublisher
{
    public Task Publish(IDomainEvent domainEvent)
    {
        Console.WriteLine($"[EVENT] {domainEvent.GetType().Name}");
        return Task.CompletedTask;
    }
}
