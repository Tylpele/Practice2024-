using Microsoft.AspNetCore.Components.Forms;
using RabbitMQ.Client;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using СhatBot.Models;
using СhatBot.RabbitMQ;

namespace СhatBot.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqService()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        public void SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);
            SendMessage(message);
        }

        public void SendMessage(string inputText, string queueName)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var message = Encoding.UTF8.GetBytes(inputText);
            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: message);
        }

        public string ReceiveMessage(string queueName)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var result = channel.BasicGet(queue: queueName, autoAck: false);


            var body = result.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            channel.BasicAck(deliveryTag: result.DeliveryTag, multiple: false);

            return message;
        }
    }
}

