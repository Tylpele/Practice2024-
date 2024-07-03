using Microsoft.AspNetCore.SignalR;
using СhatBot.RabbitMQ;

public class ChatHub : Hub
{    
    private const string NextQueue = "pre-queue";
    public Task SendMessage(string message)
    {
       RabbitMqService.SendMessage(message, NextQueue);
        Console.WriteLine("Сообщение "+ message+ " попало в очередь pre-queue");
        return Task.CompletedTask; 
    }
}
