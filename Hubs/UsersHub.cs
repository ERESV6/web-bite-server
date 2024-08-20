using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace web_bite_server.Hubs
{
    [Authorize]
    public class UsersHub : Hub
    {
    
        public override async Task OnConnectedAsync()
        {          
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {           
            await Clients.All.SendAsync("ReceiveMessage", $"Client {Context.ConnectionId} disconnected.");
            await base.OnDisconnectedAsync(exception);          
        }
    }
}