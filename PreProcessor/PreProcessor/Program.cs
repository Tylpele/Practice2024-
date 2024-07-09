//using MyPreProcessorApplication;
using NLog;
using PreProcessor1;

class Program
{
    //private  readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

    static void Main(string[] args)
    {
        NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        LogManager.LoadConfiguration("C:\\Практика\\PreProcessor\\PreProcessor\\NLog.config");
        _logger.Info("Application started");
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
           // _logger.Error("Invalid message format");
            return;
        }

        var ConnectionId = parts[0];
        var messageContent = parts[1].ToLower();

        var newMessage = $"{ConnectionId}:{messageContent}";

        RabbitMqService.SendMessage(newMessage, NextQueue);
    }
}