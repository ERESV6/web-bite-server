using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Interfaces.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Hubs
{
    [Authorize]
    public class UsersHub(UserManager<AppUser> userManager, ICardGameRepository cardGameRepository) : Hub<IUsersHub>
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ICardGameRepository _cardGameRepository = cardGameRepository;

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {

                var appUser = await _userManager.FindByNameAsync(userName);
                if (appUser != null)
                {
                    if (appUser.GameConnectionId == null)
                    {
                        appUser.GameConnection = new GameConnection
                        {
                            ConnectionId = connectionId,
                            AppUserId = appUser.Id,
                            AppUserName = appUser.UserName
                        };
                        await _userManager.UpdateAsync(appUser);
                    }
                    else
                    {
                        var gameConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(appUser.GameConnectionId);
                        if (gameConnection != null)
                        {
                            await _cardGameRepository.UpdateUserGameConnectionOnReconnect(gameConnection, connectionId);
                        }
                    }
                    await SendConnectionMessage($"{userName} has joined.");
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            var connection = await _cardGameRepository.GetGameConnectionByConnectionId(Context.ConnectionId);
            if (connection != null && userName != null)
            {
                var appUser = await _userManager.FindByNameAsync(userName);
                if (appUser != null)
                {
                    appUser.GameConnectionId = null;
                    await _userManager.UpdateAsync(appUser);
                    await _cardGameRepository.RemoveGameConnection(connection);
                    await SendConnectionMessage($"{userName} leave.");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task SendConnectionMessage(string message)
        {
            var activeUsers = await _cardGameRepository.GetAllActiveGameConnectionsAsync();
            await Clients.Others.UserConnection(message);
            await Clients.All.UserConnections(activeUsers);
        }
    }
}