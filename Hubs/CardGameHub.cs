using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Models;
using web_bite_server.Repository;

namespace web_bite_server.Hubs
{
    [Authorize]
    public class CardGameHub(UserManager<AppUser> userManager, CardGameConnectionRepository cardGameRepository) : Hub<ICardGameHub>
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly CardGameConnectionRepository _cardGameRepository = cardGameRepository;

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {

                var appUser = await _userManager.FindByNameAsync(userName);
                if (appUser != null)
                {
                    if (appUser.CardGameConnectionId == null)
                    {
                        appUser.CardGameConnection = new CardGameConnection
                        {
                            ConnectionId = connectionId,
                            AppUserId = appUser.Id,
                        };
                        await _userManager.UpdateAsync(appUser);
                    }
                    else
                    {
                        var cardGameConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
                        if (cardGameConnection != null)
                        {
                            await _cardGameRepository.UpdateUserCardGameConnectionOnReconnect(cardGameConnection, connectionId);
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
            var connection = await _cardGameRepository.GetCardGameConnectionByConnectionId(Context.ConnectionId);
            if (connection != null && userName != null)
            {
                var appUser = await _userManager.FindByNameAsync(userName);
                if (appUser != null)
                {
                    appUser.CardGameConnectionId = null;
                    await _userManager.UpdateAsync(appUser);
                    await _cardGameRepository.RemoveCardGameConnection(connection);
                    await SendConnectionMessage($"{userName} leave.");
                }
            }
            await SendConnectionMessage($"{userName} leave.");

            await base.OnDisconnectedAsync(exception);
        }

        private async Task SendConnectionMessage(string message)
        {
            var activeUsers = await _cardGameRepository.GetAllActiveCardGameConnectionsAsync();
            await Clients.Others.UserConnection(message);
            await Clients.All.UserConnections(activeUsers);
        }
    }
}