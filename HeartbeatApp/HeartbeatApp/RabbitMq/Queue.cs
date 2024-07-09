using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeartbeatApp.RabbitMq
{
    public class Queue
    {
        private static readonly ConnectionFactory _factory;
        private static readonly IConnection _connection;
        private static readonly IModel _channel;
        private static EventingBasicConsumer _consumer;

        static Queue()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
                _consumer = null;
            }
            catch (BrokerUnreachableException ex)
            {
                Console.WriteLine($"Error connecting to RabbitMQ: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public static void SendMessage(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var message = Encoding.UTF8.GetBytes("Hey! I'm alive");

            var properties = _channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, object>
            {
                { "Type", "heartbeat" }
            };

            _channel.BasicPublish(exchange: "",
                                  routingKey: queueName,
                                  basicProperties: properties,
                                  body: message);
        }

        public static void StartListening(string queueName, Action<string> onMessageReceived)
        {
            if (_consumer == null)
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _consumer = new EventingBasicConsumer(_channel);
                _consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    if (ea.BasicProperties.Headers != null &&
                        ea.BasicProperties.Headers.TryGetValue("Type", out var typeHeader))
                    {
                        var headerValue = Encoding.UTF8.GetString((byte[])typeHeader);
                        if (headerValue == "heartbeat")
                        {
                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        else
                        {
                            Console.WriteLine("Ignoring message with type: " + headerValue);
                            _channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Received message without a valid Type header: " + message);
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }

                    onMessageReceived(message);
                };

                _channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: _consumer
                );
            }
        }
    }
}
