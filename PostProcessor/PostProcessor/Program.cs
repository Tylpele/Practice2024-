//using MyPreProcessorApplication;
using NLog;
using PostProcessor;
class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    static void Main(string[] args)
    {
        LogManager.LoadConfiguration("C:\\Практика\\PostProcessor\\PostProcessor\\NLog.config");
        string CurrentQueue = "post-queue";
        try
        {
            RabbitMqService.StartListening(CurrentQueue, PostHandleMessage);
            _logger.Info("Waiting for messages. Press [enter] to exit.");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in Main: {ex.Message}");
        }
    }

    public static void PostHandleMessage(string message)
    {
        string NextQueue = "sent-queue";
        var parts = message.Split(':');
        if (parts.Length != 2)
        {
            _logger.Error("Invalid message format");
            return;
        }

        var ConnectionId = parts[0];
        var userMessage = parts[1];

        if (userMessage.Length <= 200)
        {
            try
            {
                RabbitMqService.SendMessage(message, NextQueue);
                _logger.Info($"Message sent to {NextQueue}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in PostHandleMessage");
            }
        }
        else
        {
            var partsToSend = SplitMessageBySpace(userMessage, 200);

            foreach (var part in partsToSend)
            {
                var responseMessage = $"{ConnectionId}:{part}";
                try
                {
                    RabbitMqService.SendMessage(responseMessage, NextQueue);
                    _logger.Info($"User message sent to {NextQueue}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in PostHandleMessage");
                }
            }
        }
    }

    private static List<string> SplitMessageBySpace(string message, int maxLength)
    {
        var result = new List<string>();
        int start = 0;

        while (start < message.Length)
        {
            int length = Math.Min(maxLength, message.Length - start);
            int end = start + length;

            if (end < message.Length && message[end] != ' ')
            {
                int lastSpace = message.LastIndexOf(' ', end);
                if (lastSpace > start)
                {
                    length = lastSpace - start;
                }
            }

            result.Add(message.Substring(start, length).Trim());
            start += length;
        }

        return result;
    }


}