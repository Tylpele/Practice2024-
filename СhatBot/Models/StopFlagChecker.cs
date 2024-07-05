using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class StopFlagChecker: BackgroundService
{

    private Timer timer;
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {


        // Create a timer to send messages periodically
        timer = new Timer(state => CheckFlag(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        // Wait indefinitely until cancellation is requested
        return Task.CompletedTask;
    }
    private void CheckFlag()
    {
        if (File.Exists("C:\\Практика\\ChatBot\\ChatBot\\stop.flag"))
        {
            Console.WriteLine("Stop flag detected. Stopping application...");
            Environment.Exit(0);
        }

    }
}
