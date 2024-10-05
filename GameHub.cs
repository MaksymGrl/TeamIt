using Microsoft.AspNetCore.SignalR;
namespace TeamIt;

public class GameHub : Hub
{
    public async Task JoinGameGroup(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
    }

    public async Task PlayerJoined(string gameId)
    {
        await Clients.Group(gameId).SendAsync("PlayerJoined");
    }

    public async Task SendMove(string gameId, int x, int y)
    {
        await Clients.Group(gameId).SendAsync("ReceiveMove", x, y);
    }
}