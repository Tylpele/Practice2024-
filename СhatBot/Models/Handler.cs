using Microsoft.Extensions.Hosting;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using СhatBot.RabbitMQ;
using System.Threading;
using System.Threading.Tasks;

namespace СhatBot.Models
{
    public class Handler : BackgroundService
    {
        private readonly string CurrentQueue = "queue";
        private readonly string NextQueue = "post-queue";
        private readonly RabbitMqService _rabbitMqService;
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, string> _responses;

        public Handler(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
            _responses = LoadResponses("responses.json");
        }

        private Dictionary<string, string> LoadResponses(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.Error("JSON file not found");
                return new Dictionary<string, string>();
            }
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        public void HandleMessage(string message)
        {
            var parts = message.Split(':');
            if (parts.Length != 2)
            {
                _logger.Error("Invalid message format: " + message);
                return;
            }

            var ConnectionId = parts[0];
            var messageContent = parts[1];

            if (_responses.TryGetValue(messageContent, out var answer))
            {
                var responseMessage = $"{ConnectionId}:{answer}";
                RabbitMqService.SendMessage(responseMessage, NextQueue);
                _logger.Info($"Message '{messageContent}' processed and response '{answer}' was sent to {NextQueue}");
            }
            else
            {
                var responseMessage = $"{ConnectionId}:I don't know the answer";
                RabbitMqService.SendMessage(responseMessage, NextQueue);
                _logger.Warn($"No response found for message '{messageContent}'");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            RabbitMqService.StartListening(CurrentQueue, HandleMessage);
            return Task.CompletedTask;
        }
    }
}
