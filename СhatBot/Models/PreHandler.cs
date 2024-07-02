using СhatBot.RabbitMQ;

namespace СhatBot.Models
{
    public class PreHandler
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly Handler _handler;

        public PreHandler()
        {
            _rabbitMqService = new RabbitMqService();
            _handler = new Handler();
        }

        public void PreHandl(string queueName)
        {
            string nextQueue = "queue";
            string message = _rabbitMqService.ReceiveMessage(queueName);
            Console.WriteLine("Сообщение " + message + " попало в очередь на нормализацию");

            string newMessage = message.ToLower();


            _rabbitMqService.SendMessage(newMessage, nextQueue);
            Console.WriteLine("Сообщение " + message + " передалось в очередь на обработку");
            _handler.Handl(nextQueue);
        }
    }
}
