using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Text.Json;

public class RabbitMqService
{
    private static readonly ConnectionFactory _factory;
    private static readonly IConnection _connection;
    private static readonly IModel _channel;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    static RabbitMqService()
    {
        _factory = new ConnectionFactory { HostName = "localhost" };
        try
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.Error($"Error connecting to RabbitMQ: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error: {ex.Message}");
        }
    }

    public static void StartListening(string queueName, Action<string> onMessageReceived)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            if (ea.BasicProperties.Headers != null &&
                ea.BasicProperties.Headers.TryGetValue("Type", out var typeHeader) &&
                typeHeader is byte[] headerBytes &&
                Encoding.UTF8.GetString(headerBytes) == "userMessage")
            {
                _channel.BasicAck(ea.DeliveryTag, false);
                onMessageReceived(message);
                _logger.Info("Message for user was sent to queue");

            }
            else
            {
                _channel.BasicNack(ea.DeliveryTag, false, true);
                _logger.Info("System Message was sent to queue");
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );
        _logger.Info($"Started listening to {queueName}");
    }

    public static void SendMessage(string inputText, string queueName)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var properties = _channel.CreateBasicProperties();
        properties.Headers = new Dictionary<string, object>
            {
                { "Type", Encoding.UTF8.GetBytes("userMessage") }
            };

        var message = Encoding.UTF8.GetBytes(inputText);
        _channel.BasicPublish(exchange: "",
                              routingKey: queueName,
                              basicProperties: properties,
                              body: message);
    }
}