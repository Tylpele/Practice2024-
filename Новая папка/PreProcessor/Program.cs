//using MyPreProcessorApplication;
using NLog;
using PreProcessor1;

class Program
{
    //private readonly string CurrentQueue = "pre-queue";
    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
    static void Main(string[] args)
    {
        string CurrentQueue = "pre-queue";
        RabbitMqService.StartListening(CurrentQueue, PreHandleMessage);
        while (true)
        { 
            Thread.Sleep(10000);
        }
    }
    private static void PreHandleMessage(string message)
    {
        string NextQueue = "queue";
        var parts = message.Split(':');
        if (parts.Length != 2)
        {
            //_logger.Error("Invalid message format: " + message);
            return;
        }

        var ConnectionId = parts[0];
        var messageContent = parts[1].ToLower();

        var newMessage = $"{ConnectionId}:{messageContent}";

        RabbitMqService.SendMessage(newMessage, NextQueue);
        Console.WriteLine("Message was sent to queue");
        //_logger.Info("Message " + message + " was sent to queue");
    }
}