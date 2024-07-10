using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class StopFlagChecker : BackgroundService
{
    private IHubContext<ChatHub> _hubContext;
    public StopFlagChecker(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }
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
        ManageErrorsScreens(File.Exists("stop.flag")
                        );
    }
    private void ManageErrorsScreens(bool switchToError)
    {
        if (switchToError)
        {
            Console.WriteLine("Stop flag detected. Stopping application...");
        }
        _hubContext.Clients.All.SendAsync("SwitchErrorScreens", switchToError);
    }
}
