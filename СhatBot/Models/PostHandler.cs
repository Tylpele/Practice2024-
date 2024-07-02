using Microsoft.AspNetCore.SignalR;
using СhatBot.RabbitMQ;

namespace СhatBot.Models
{
    public class PostHandler: BackgroundService
    {
        private readonly RabbitMqService _rabbitMqService;
        IHubContext<ChatHub> _hubContext;
        public PostHandler()
        {
            _rabbitMqService = new RabbitMqService();
            
        }
        public void PostHandl(string queueName)
        {
            string message = _rabbitMqService.ReceiveMessage(queueName);
            Console.WriteLine("Сообщение " + message + " попало в очередь на отправку обратно");
           // _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
