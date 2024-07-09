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

    public void StartListening(string queueName, Action<string> handleMessage)
    {
        _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Логирование заголовков сообщения
            if (ea.BasicProperties.Headers != null)
            {
                foreach (var header in ea.BasicProperties.Headers)
                {
                    var value = header.Value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : header.Value.ToString();
                    _logger.Info($"Header: {header.Key} = {value}");
                }
            }

            handleMessage(message);
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    public void SendMessage(string message, string queueName)
    {
        var properties = _channel.CreateBasicProperties();
        properties.Headers = new Dictionary<string, object>
            {
                { "Type", Encoding.UTF8.GetBytes("userMessage") }
            };

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);

        // Логирование отправленного сообщения и его заголовков
        _logger.Info($"Sent message to queue {queueName}: {message}");
        foreach (var header in properties.Headers)
        {
            var value = header.Value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : header.Value.ToString();
            _logger.Info($"Sent header: {header.Key} = {value}");
        }
    }
}