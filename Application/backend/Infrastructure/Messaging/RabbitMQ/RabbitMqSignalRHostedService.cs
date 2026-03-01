using System.Text;
using System.Text.Json;
using backend.Api.Hubs;
using backend.Domain.Events;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace backend.Infrastructure.Messaging.RabbitMQ;

/// <summary>
/// Hosted servis koji sluša RabbitMQ "domain-events" exchange i šalje
/// real-time notifikacije klijentima preko SignalR (DocumentHub).
/// </summary>
public class RabbitMqSignalRHostedService : IHostedService, IDisposable
{
    private const string ExchangeName = "domain-events";
    private readonly IConfiguration _configuration;
    private readonly IHubContext<DocumentHub> _hubContext;
    private readonly ILogger<RabbitMqSignalRHostedService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private string? _queueName;
    private EventingBasicConsumer? _consumer;

    public RabbitMqSignalRHostedService(
        IConfiguration configuration,
        IHubContext<DocumentHub> hubContext,
        ILogger<RabbitMqSignalRHostedService> logger)
    {
        _configuration = configuration;
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var factory = new ConnectionFactory { HostName = hostName };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true);

            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(
                queue: _queueName,
                exchange: ExchangeName,
                routingKey: "");

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += OnMessageReceived;

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: _consumer);

            _logger.LogInformation("RabbitMQ SignalR consumer pokrenut, exchange: {Exchange}", ExchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ nije dostupan – SignalR consumer nije pokrenut.");
        }

        return Task.CompletedTask;
    }

    private void OnMessageReceived(object? sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            HandleMessageAsync(json, ea.DeliveryTag).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri obradi RabbitMQ poruke.");
        }
        finally
        {
            _channel?.BasicAck(ea.DeliveryTag, false);
        }
    }

    private async Task HandleMessageAsync(string json, ulong deliveryTag)
    {
        DomainEventEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<DomainEventEnvelope>(json);
        }
        catch
        {
            _logger.LogWarning("Neispravan envelope format: {Json}", json);
            return;
        }

        if (envelope == null || string.IsNullOrEmpty(envelope.EventType))
        {
            _logger.LogWarning("Prazan envelope ili EventType.");
            return;
        }

        switch (envelope.EventType)
        {
            case nameof(JobUpdatedEvent):
                var jobUpdated = JsonSerializer.Deserialize<JobUpdatedEvent>(envelope.Payload);
                if (jobUpdated != null)
                {
                    var groupName = $"job:{jobUpdated.JobId}";
                    await _hubContext.Clients
                        .Group(groupName)
                        .SendAsync("JobUpdated", jobUpdated.JobId, jobUpdated.OccurredAt);
                    _logger.LogDebug("SignalR JobUpdated poslato grupi {Group}", groupName);
                }
                break;
            default:
                _logger.LogDebug("Nepoznat EventType: {EventType}", envelope.EventType);
                break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
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
