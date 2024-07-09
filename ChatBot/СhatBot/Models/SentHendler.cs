using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using NLog;
using СhatBot.RabbitMQ;
using System.Threading;
using System.Threading.Tasks;

namespace СhatBot.Models
{
    public class SentHendler : BackgroundService
    {
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly string CurrentQueue = "sent-queue";
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public SentHendler(IHubContext<ChatHub> chatHub)
        {
            _chatHub = chatHub;
        }

        public async void PostHandleMessage(string message)
        {
            var parts = message.Split(':');
            if (parts.Length != 2)
            {
                _logger.Error("Invalid message format: " + message);
                return;
            }

            var ConnectionId = parts[0];
            var userMessage = parts[1];

            await _chatHub.Clients.Client(ConnectionId).SendAsync("ReceiveMessage", userMessage);
            _logger.Info($"Message to connection {ConnectionId}: {userMessage} was sent user");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            RabbitMqService.StartListening(CurrentQueue, PostHandleMessage);

            return Task.CompletedTask;
        }
    }
}
