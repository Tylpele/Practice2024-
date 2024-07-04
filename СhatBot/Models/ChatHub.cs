using Microsoft.AspNetCore.SignalR;
using NLog;
using СhatBot.RabbitMQ;

public class ChatHub : Hub
{    
    private const string NextQueue = "pre-queue";
    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
    public async Task SendMessageToQueue(string formattedMessage)
    {
        RabbitMqService.SendMessage(formattedMessage, "pre-queue");
        _logger.Info($"Message sent to queue: {formattedMessage}");
    }
}
