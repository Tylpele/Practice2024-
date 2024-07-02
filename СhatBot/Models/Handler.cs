using Microsoft.AspNetCore.SignalR;
using СhatBot.RabbitMQ;

namespace СhatBot.Models
{
    public class Handler: BackgroundService
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly PostHandler _postHandler;
       

        public Handler()
        {
            _rabbitMqService = new RabbitMqService();
            _postHandler = new PostHandler();

        }
        public void Handl(string queueName)
        {
            string nextQueue = "post-queue";
            string message = _rabbitMqService.ReceiveMessage(queueName);
            Console.WriteLine("Сообщение " + message+" попало в очередь на обработку");

            _rabbitMqService.SendMessage(message, nextQueue);
            Console.WriteLine("Сообщение " + message + " передалось в очередь на обратную отправку");
            _postHandler.PostHandl(nextQueue);

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
