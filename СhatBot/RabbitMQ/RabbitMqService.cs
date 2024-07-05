using Microsoft.AspNetCore.Components.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using СhatBot.Models;
using СhatBot.RabbitMQ;

namespace СhatBot.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        private static readonly ConnectionFactory _factory;
        private static readonly IConnection _connection;
        private static readonly IModel _channel;

        static RabbitMqService()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);
            SendMessage(message);
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
                { "Type", "userMessage" }
            };

            var message = Encoding.UTF8.GetBytes(inputText);
            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties, // Установка заголовков
                                 body: message);
        }

        public static void StartListening(string queueName, Action<string> onMessageReceived)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Проверка заголовков сообщений
                if (ea.BasicProperties.Headers != null &&
                    ea.BasicProperties.Headers.TryGetValue("Type", out var typeHeader) &&
                    typeHeader is byte[] headerBytes &&
                    Encoding.UTF8.GetString(headerBytes) == "userMessage")
                {
                    onMessageReceived(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            _channel.BasicConsume(queue: queueName,
                     autoAck: false,
                     consumer: consumer);
        }
    }
}
