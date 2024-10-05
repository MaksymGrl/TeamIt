using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamIt.Controllers;


public class GameCleanupService : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 1 minutes
    private readonly TimeSpan _inactivityLimit = TimeSpan.FromMinutes(30); // Inactivity limit of 30 minutes

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            CleanupInactiveGames();
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private void CleanupInactiveGames()
    {
        Console.WriteLine("CleanupCheck");
        /*
        var now = DateTime.UtcNow;

        lock (GameController.Games) // Ensure thread safety when modifying the shared Games list
        {
            GameController.Games.RemoveAll(game =>
                game.Players.Count == 0 || // Remove games with no players
                (now - game.LastMoveTime) > _inactivityLimit); // Remove games with no moves for 30 minutes
        }
        */
    }
}