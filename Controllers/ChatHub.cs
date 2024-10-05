using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    // Method to add the user to a conversation group
    public async Task JoinConversationGroup(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    // Method to send a message to the specific conversation group
    public async Task SendMessageToGroup(string conversationId, string user, string message)
    {
        await Clients.Group(conversationId).SendAsync("ReceiveMessage", user, message);
    }
}