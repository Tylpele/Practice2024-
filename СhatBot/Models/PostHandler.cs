using Microsoft.AspNetCore.SignalR;
using СhatBot.RabbitMQ;

namespace СhatBot.Models
{
    public class PostHandler: BackgroundService
    {

        IHubContext<ChatHub> _chatHub;
        private readonly string CurrentQueue = "post-queue";
        public PostHandler(IHubContext<ChatHub> chatHub)
        {
            _chatHub = chatHub;
            
        }
        public void PostHandleMessage(string message)
        {
            _chatHub.Clients.All.SendAsync("ReceiveMessage", message);
            Console.WriteLine("Сообщение " + message + " выведено из очередей");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            RabbitMqService.StartListening(CurrentQueue, PostHandleMessage);

            return Task.CompletedTask;
        }
    }
}
