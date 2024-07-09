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
    public class RabbitMqService: IRabbitMqService
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

        public static void SendMessage(object obj, string queueName)
        {
            var message = JsonSerializer.Serialize(obj);
            SendMessage(message, queueName);
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
                { "Type", Encoding.UTF8.GetBytes("userMessage") } // Преобразование строки в byte[]
            };

            Console.WriteLine("Message Headers:");
            foreach (var header in properties.Headers)
            {
                Console.WriteLine($"{header.Key}: {Encoding.UTF8.GetString((byte[])header.Value)}");
            }

            var message = Encoding.UTF8.GetBytes(inputText);
            _channel.BasicPublish(exchange: "",
                                  routingKey: queueName,
                                  basicProperties: properties,
                                  body: message);
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
                }
                else
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
        }
    }
}
