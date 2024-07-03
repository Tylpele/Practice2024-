using Microsoft.AspNetCore.SignalR;
using СhatBot.RabbitMQ;

namespace СhatBot.Models
{
    public class Handler: BackgroundService
    {

        private readonly string CurrentQueue = "queue";
        private readonly string NextQueue = "post-queue";



        public void HandleMessage(string message)
        {
            char[] temp = message.ToCharArray();
            Array.Reverse(temp);
            string answer = new string(temp);

            RabbitMqService.SendMessage(answer, NextQueue);
            Console.WriteLine("Сообщение "+ message+ " отправлено в очередь post-queue");

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            RabbitMqService.StartListening(CurrentQueue, HandleMessage);
            return Task.CompletedTask;
        }
    }
}
