using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using СhatBot.RabbitMQ;

namespace СhatBot.Models
{
    public class PreHandler: BackgroundService
    {

        private readonly string CurrentQueue = "pre-queue";
        private readonly string NextQueue = "queue";

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            RabbitMqService.StartListening(CurrentQueue, PreHandleMessage);
            return Task.CompletedTask;
        }

        private void PreHandleMessage(string message)
        {
            string newMessage = message.ToLower();

            RabbitMqService.SendMessage(newMessage, NextQueue);
            Console.WriteLine("Сообщение " + message+ " отправлено в очередь queue");
        }
    }
}
