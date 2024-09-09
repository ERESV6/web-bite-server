
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Interfaces.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Services.CardGame
{
    public class CardGameConnectionService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICardGameConnectionRepository _cardGameConnectionRepository;
        private readonly IHubContext<CardGameHub, ICardGameHub> _hubContext;

        public CardGameConnectionService
        (
            UserManager<AppUser> userManager,
            ICardGameConnectionRepository cardGameConnectionRepository,
            IHubContext<CardGameHub, ICardGameHub> hubContext
        )
        {
            _userManager = userManager;
            _cardGameConnectionRepository = cardGameConnectionRepository;
            _hubContext = hubContext;
        }

        public async Task<bool> RequestCardGameConnection(string userToConnectionId, ClaimsPrincipal user)
        {
            var userConnection = await GetLoggedUserGameConnection(user);
            var userToConnection = await _cardGameConnectionRepository.GetCardGameConnectionByConnectionId(userToConnectionId);
            if (
                userConnection != null && string.IsNullOrEmpty(userConnection.UserToId) && string.IsNullOrEmpty(userConnection.UserToRequestPendingId) &&
                userToConnection != null && string.IsNullOrEmpty(userToConnection.UserToId) && string.IsNullOrEmpty(userToConnection.UserToRequestPendingId)
                )
            {
                await _cardGameConnectionRepository.UpdateConnectionUsersPendingIds(userConnection, userToConnection);
                await _hubContext.Clients.Client(userToConnection.ConnectionId).RequestCardGameConnection(userConnection.AppUser?.UserName ?? "");

                var allActiveCardGameConnections = await _cardGameConnectionRepository.GetAllActiveCardGameConnectionsAsync();
                await _hubContext.Clients.All.UserConnections(allActiveCardGameConnections);
                return true;
            }
            return false;
        }

        public async Task<bool> DeclineCardGameConnection(ClaimsPrincipal user)
        {
            var userConnection = await GetLoggedUserGameConnection(user);
            if (string.IsNullOrEmpty(userConnection?.UserToRequestPendingId))
            {
                throw new Exception("USER IS NOT WAITING FOR CONNECTION");
            }

            var appPendingUser = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (appPendingUser?.CardGameConnectionId == null)
            {
                throw new Exception("PENDING USER NOT FOUND OR IS NOT CONNECTED TO THE GAME");
            }

            var userToConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appPendingUser.CardGameConnectionId);
            if (string.IsNullOrEmpty(userToConnection?.UserToRequestPendingId))
            {
                throw new Exception("PENDING USER IS NOT WAITING FOR CONNECTION");
            }

            if (
                userConnection != null && string.IsNullOrEmpty(userConnection.UserToId) &&
                userToConnection != null && string.IsNullOrEmpty(userToConnection.UserToId)
                )
            {
                await _cardGameConnectionRepository.CleanConnectionUsersPendingIds(userConnection, userToConnection);
                await _hubContext.Clients.Client(userToConnection.ConnectionId).DeclineCardGameConnection();

                var allActiveCardGameConnections = await _cardGameConnectionRepository.GetAllActiveCardGameConnectionsAsync();
                await _hubContext.Clients.All.UserConnections(allActiveCardGameConnections);
                return true;
            }
            return false;
        }

        public async Task<bool> AcceptCardGameConnection(ClaimsPrincipal user)
        {
            var userConnection = await GetLoggedUserGameConnection(user);
            if (string.IsNullOrEmpty(userConnection?.UserToRequestPendingId))
            {
                throw new Exception("USER IS NOT WAITING FOR CONNECTION");
            }
            if (!string.IsNullOrEmpty(userConnection.UserToId))
            {
                throw new Exception("USER IS ALREADY CONNECTED WITH OTHER USER");
            }

            var userReceive = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (userReceive?.CardGameConnectionId == null)
            {
                throw new Exception("GAME REQUESTING USER NOT FOUNDOR IS NOT CONNECTED TO THE GAME");
            }

            var userReceiveCardGameConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(userReceive.CardGameConnectionId);
            if (string.IsNullOrEmpty(userReceiveCardGameConnection?.UserToRequestPendingId))
            {
                throw new Exception("GAME REQUESTING USER NOT FOUND OR IS NOT WAITING FOR CONNECTION");
            }
            if (!string.IsNullOrEmpty(userReceiveCardGameConnection?.UserToId))
            {
                throw new Exception("GAME REQUESTING USER IS ALREADY CONNECTED WITH OTHER USER");
            }

            if (userConnection != null && userReceiveCardGameConnection != null)
            {
                await _cardGameConnectionRepository.ConnectConnectionUsersIds(userConnection, userReceiveCardGameConnection);
                await _hubContext.Clients.Client(userReceiveCardGameConnection.ConnectionId).AcceptCardGameConnection();
                return true;
            }

            return false;
        }

        public async Task<CardGameConnectionDto> CheckCardGameConnection(ClaimsPrincipal user)
        {
            var userCardGameConnection = await GetLoggedUserGameConnection(user);
            if (userCardGameConnection?.UserToId == null)
            {
                throw new Exception("USER NOT CONNECTED WITH OTHER USER");
            }

            var appConnectedUser = await _userManager.FindByIdAsync(userCardGameConnection.UserToId);
            if (appConnectedUser?.CardGameConnectionId == null)
            {
                throw new Exception("CONNECTED USER NOT FOUND OR IS NOT CONNECTED TO THE GAME");
            }

            var connectedUserCardGameConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appConnectedUser.CardGameConnectionId);
            if (connectedUserCardGameConnection?.UserToId == null)
            {
                throw new Exception("CONNECTED USER NOT CONNECTED WITH OTHER USER");
            }
            if (userCardGameConnection.UserToId != appConnectedUser.Id || connectedUserCardGameConnection.UserToId != userCardGameConnection.AppUser?.Id)
            {
                throw new Exception("USER AND CONNECTED USER IDS DONT MATCH");
            }
            return new CardGameConnectionDto
            {
                ConnectionId = userCardGameConnection.ConnectionId,
                UserName = userCardGameConnection.AppUser.UserName ?? "",
                EnemyUserName = connectedUserCardGameConnection.AppUser?.UserName ?? ""
            };
        }

        public async Task<CardGameConnection?> GetLoggedUserGameConnection(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser?.CardGameConnectionId == null)
            {
                throw new Exception("USER IS NOT CONNECTED TO THE GAME");
            }

            return await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
        }
    }
}