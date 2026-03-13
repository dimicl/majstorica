using backend.Domain.Events;

namespace backend.Application.Interfaces;

public interface IMessagePublisher
{
    Task Publish(DomainEvent domainEvent);
}
