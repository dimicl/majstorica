using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace backend.Infrastructure.Messaging.RabbitMQ;

public class RabbitMqConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string ExchangeName = "domain-events";
    private readonly string _queueName;

    public RabbitMqConsumer()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true
        );

        _queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(
            queue: _queueName,
            exchange: ExchangeName,
            routingKey: ""
        );
    }

    public void Start()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"[RabbitMQ] Event received: {message}");
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: true,
            consumer: consumer
        );
    }
}
