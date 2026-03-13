using System.Text;
using System.Text.Json;
using backend.Application.Interfaces;
using backend.Domain.Events;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace backend.Infrastructure.Messaging.RabbitMQ;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private const string ExchangeName = "domain-events";
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = hostName };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true
        );
    }

    public Task Publish(DomainEvent domainEvent)
    {
        var payload = JsonSerializer.Serialize(domainEvent);
        var envelope = new {
            EventType = domainEvent.GetType().Name,
            Payload = payload
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
