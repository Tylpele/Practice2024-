using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using СhatBot.RabbitMQ;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace СhatBot.Models
{
    public class PreProcessor : BackgroundService
    {
        private readonly string CurrentQueue = "pre-queue";
        private readonly string NextQueue = "queue";
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            RabbitMqService.StartListening(CurrentQueue, PreHandleMessage);
            return Task.CompletedTask;
        }

        private void PreHandleMessage(string message)
        {
            var parts = message.Split(':');
            if (parts.Length != 2)
            {
                _logger.Error("Invalid message format: " + message);
                return;
            }

            var ConnectionId = parts[0];
            var messageContent = parts[1].ToLower();

            var newMessage = $"{ConnectionId}:{messageContent}";

            RabbitMqService.SendMessage(newMessage, NextQueue);
            _logger.Info("Message " + message + " was sent to queue");
        }
    }
}
