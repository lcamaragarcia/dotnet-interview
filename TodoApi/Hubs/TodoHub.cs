using Microsoft.AspNetCore.SignalR;

namespace TodoApi.Hubs;

public class TodoHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
