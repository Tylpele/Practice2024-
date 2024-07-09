using NLog;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;
using System.Text.Json;

public class Program
{
    private readonly Dictionary<string, string> _responses;
    private static readonly string NextQueue = "post-queue";
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly ConnectionFactory _factory;
    private static readonly IConnection _connection;
    private static readonly IModel _channel;
    private readonly RabbitMqService _rabbitMqService;

    static Program()
    {
        _factory = new ConnectionFactory { HostName = "localhost" };
        try
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: NextQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }
        catch (BrokerUnreachableException ex)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Error($"Error connecting to RabbitMQ: {ex.Message}");
        }
        catch (Exception ex)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Error($"Unexpected error: {ex.Message}");
        }
    }

    public Program(RabbitMqService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
        _responses = LoadResponses("C:\\Практика\\Processor\\Processor\\responses.json");
    }

    static void Main(string[] args)
    {
        var rabbitMqService = new RabbitMqService();
        var program = new Program(rabbitMqService);

        string CurrentQueue = "queue";
        rabbitMqService.StartListening(CurrentQueue, program.HandleMessage);

        Console.WriteLine("Listening to messages. Press [enter] to exit.");
       Console.ReadKey();
    }

    private Dictionary<string, string> LoadResponses(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("JSON file not found");
            return new Dictionary<string, string>();
        }
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    public void HandleMessage(string message)
    {
        var parts = message.Split(':');

        if (parts.Length != 2)
        {
            _logger.Error("Invalid message format: " + message);
            return;
        }

        var ConnectionId = parts[0];
        var messageContent = parts[1];

        if (_responses.TryGetValue(messageContent, out var answer))
        {
            var responseMessage = $"{ConnectionId}:{answer}";
            _rabbitMqService.SendMessage(responseMessage, NextQueue);
            _logger.Info($"Message '{messageContent}' processed and response '{answer}' was sent to {NextQueue}");
            Console.WriteLine("Message was sent to post-queue");
        }
        else
        {
            var responseMessage = $"{ConnectionId}:I don't know the answer";
            _rabbitMqService.SendMessage(responseMessage, NextQueue);
            _logger.Warn($"No response found for message '{messageContent}'");
            Console.WriteLine("Message was sent to post-queue with default response");
        }
    }
}