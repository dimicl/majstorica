using System.Text;
using System.Text.Json;
using backend.Application.Interfaces;
using backend.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace backend.Infrastructure.Messaging.RabbitMQ;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private const string ExchangeName = "domain-events";
    private readonly string _hostName;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        _logger = logger;
    }

    private IModel GetChannel()
    {
        if (_channel is { IsOpen: true })
            return _channel;

        lock (_lock)
        {
            if (_channel is { IsOpen: true })
                return _channel;

            try
            {
                _connection?.Dispose();
                var factory = new ConnectionFactory { HostName = _hostName };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(
                    exchange: ExchangeName,
                    type: ExchangeType.Fanout,
                    durable: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ nije dostupan — poruka se preskače.");
                throw;
            }
        }

        return _channel!;
    }

    public Task Publish(DomainEvent domainEvent)
    {
        try
        {
            var channel = GetChannel();

            var payload = JsonSerializer.Serialize(domainEvent);
            var envelope = new
            {
                EventType = domainEvent.GetType().Name,
                Payload = payload
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

            channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: "",
                basicProperties: null,
                body: body);
        }
        catch (Exception ex)
        {
            // RabbitMQ nije dostupan — logujemo ali ne padamo
            _logger.LogWarning(ex, "Nije moguće objaviti domain event {EventType}.", domainEvent.GetType().Name);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}