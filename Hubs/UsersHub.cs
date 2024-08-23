using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.Dtos.CardGame;
using web_bite_server.interfaces.CardGame;

namespace web_bite_server.Hubs
{
    [Authorize]
    public class UsersHub : Hub<IUsersHub>
    {
        private static readonly List<CardGameActiveUserDto> SignalRUsers = [];
        
        public override async Task OnConnectedAsync()
        {          
            var id = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name;
            if (!SignalRUsers.Any(x => x.ConnectionId == id) && userName != null)
            {
                SignalRUsers.Add(new CardGameActiveUserDto{ ConnectionId = id, UserName = userName });
            }
            await Clients.All.UserConnection($"{userName} has joined", SignalRUsers);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {           
            var item = SignalRUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                SignalRUsers.Remove(item);
                await Clients.All.UserConnection($"{item.UserName} disconnected.", SignalRUsers);
            }
            await base.OnDisconnectedAsync(exception);          
        }
    }
}