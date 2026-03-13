using System.Text;
using System.Text.Json;
using backend.Api.Hubs;
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
            _logger.LogDebug("Primljena RabbitMQ poruka: {Json}", json);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var eventType = root.TryGetProperty("EventType", out var et) ? et.GetString() : null;
            var payloadStr = root.TryGetProperty("Payload", out var pl) ? pl.GetString() : null;
            if (string.IsNullOrEmpty(eventType) || string.IsNullOrEmpty(payloadStr))
                return;

            ForwardToSignalR(eventType, payloadStr);
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

    /// <summary>
    /// Za poznate tipove domain eventa šalje notifikaciju odgovarajućim korisnicima preko SignalR.
    /// Frontend može slušati npr. "JobAssigned", "JobCompleted", "JobPublished".
    /// </summary>
    private void ForwardToSignalR(string eventType, string payloadJson)
    {
        try
        {
            var payloadObj = JsonSerializer.Deserialize<object>(payloadJson);
            using var payloadDoc = JsonDocument.Parse(payloadJson);
            var p = payloadDoc.RootElement;

            switch (eventType)
            {
                case "JobAssignedEvent":
                    if (p.TryGetProperty("ClientUserId", out var clientIdEl) &&
                        Guid.TryParse(clientIdEl.GetString(), out var clientId))
                    {
                        _ = _hubContext.Clients.User(clientId.ToString()).SendAsync("JobAssigned", payloadObj);
                    }
                    if (p.TryGetProperty("AssignedMasterId", out var masterIdEl) &&
                        masterIdEl.ValueKind != JsonValueKind.Null &&
                        Guid.TryParse(masterIdEl.GetString(), out var masterId) && masterId != Guid.Empty)
                    {
                        _ = _hubContext.Clients.User(masterId.ToString()).SendAsync("JobAssigned", payloadObj);
                    }
                    break;
                case "JobCompletedEvent":
                    if (p.TryGetProperty("ClientUserId", out var cIdEl) &&
                        Guid.TryParse(cIdEl.GetString(), out var cId))
                    {
                        _ = _hubContext.Clients.User(cId.ToString()).SendAsync("JobCompleted", payloadObj);
                    }
                    if (p.TryGetProperty("AssignedMasterId", out var mIdEl) && mIdEl.ValueKind != JsonValueKind.Null)
                    {
                        var mStr = mIdEl.GetString();
                        if (!string.IsNullOrEmpty(mStr) && Guid.TryParse(mStr, out var mId) && mId != Guid.Empty)
                            _ = _hubContext.Clients.User(mId.ToString()).SendAsync("JobCompleted", payloadObj);
                    }
                    break;
                case "JobPublishedEvent":
                    _logger.LogDebug("JobPublishedEvent primljen, JobId: {JobId}", p.TryGetProperty("JobId", out var j) ? j.GetString() : null);
                    break;
                default:
                    _logger.LogDebug("RabbitMQ event tip {EventType} nije mapiran na SignalR.", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR forward za {EventType} nije uspeo.", eventType);
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
