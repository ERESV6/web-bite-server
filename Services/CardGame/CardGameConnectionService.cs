
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Exceptions;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Models;
using web_bite_server.Repository;

namespace web_bite_server.Services.CardGame
{
    public class CardGameConnectionService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CardGameConnectionRepository _cardGameConnectionRepository;
        private readonly IHubContext<CardGameHub, ICardGameHub> _hubContext;

        public CardGameConnectionService
        (
            UserManager<AppUser> userManager,
            CardGameConnectionRepository cardGameConnectionRepository,
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
                throw new NotFoundException("USER IS NOT WAITING FOR CONNECTION");
            }

            var appPendingUser = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (appPendingUser?.CardGameConnectionId == null)
            {
                throw new NotFoundException("PENDING USER NOT FOUND OR IS NOT CONNECTED TO THE GAME");
            }

            var userToConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appPendingUser.CardGameConnectionId);
            if (string.IsNullOrEmpty(userToConnection?.UserToRequestPendingId))
            {
                throw new NotFoundException("PENDING USER IS NOT WAITING FOR CONNECTION");
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
                throw new NotFoundException("USER IS NOT WAITING FOR CONNECTION");
            }
            if (!string.IsNullOrEmpty(userConnection.UserToId))
            {
                throw new NotFoundException("USER IS ALREADY CONNECTED WITH OTHER USER");
            }

            var userReceive = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (userReceive?.CardGameConnectionId == null)
            {
                throw new NotFoundException("GAME REQUESTING USER NOT FOUNDOR IS NOT CONNECTED TO THE GAME");
            }

            var userReceiveCardGameConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(userReceive.CardGameConnectionId);
            if (string.IsNullOrEmpty(userReceiveCardGameConnection?.UserToRequestPendingId))
            {
                throw new NotFoundException("GAME REQUESTING USER NOT FOUND OR IS NOT WAITING FOR CONNECTION");
            }
            if (!string.IsNullOrEmpty(userReceiveCardGameConnection?.UserToId))
            {
                throw new NotFoundException("GAME REQUESTING USER IS ALREADY CONNECTED WITH OTHER USER");
            }

            if (userConnection != null && userReceiveCardGameConnection != null)
            {
                await _cardGameConnectionRepository.ConnectConnectionUsersIds(userConnection, userReceiveCardGameConnection);
                await _hubContext.Clients.Client(userReceiveCardGameConnection.ConnectionId).AcceptCardGameConnection();
                return true;
            }

            return false;
        }

        public async Task<CardGameUsersConnectionDto> CheckCardGameConnection(ClaimsPrincipal user)
        {
            var userCardGameConnection = await GetLoggedUserGameConnection(user);
            if (userCardGameConnection?.UserToId == null)
            {
                throw new NotFoundException("USER NOT CONNECTED WITH OTHER USER");
            }

            var appConnectedUser = await _userManager.FindByIdAsync(userCardGameConnection.UserToId);
            if (appConnectedUser?.CardGameConnectionId == null)
            {
                throw new NotFoundException("CONNECTED USER NOT FOUND OR IS NOT CONNECTED TO THE GAME");
            }

            var connectedUserCardGameConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appConnectedUser.CardGameConnectionId);
            if (connectedUserCardGameConnection?.UserToId == null)
            {
                throw new NotFoundException("CONNECTED USER NOT CONNECTED WITH OTHER USER");
            }
            if (userCardGameConnection.UserToId != appConnectedUser.Id || connectedUserCardGameConnection.UserToId != userCardGameConnection.AppUser?.Id)
            {
                throw new NotFoundException("USER AND CONNECTED USER IDS DONT MATCH");
            }
            return new CardGameUsersConnectionDto
            {
                UserConnection = userCardGameConnection,
                EnemyUserConnection = connectedUserCardGameConnection
            };
        }

        public async Task<CardGameConnection?> GetLoggedUserGameConnection(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser?.CardGameConnectionId == null)
            {
                throw new NotFoundException("USER IS NOT CONNECTED TO THE GAME");
            }

            return await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
        }
    }
}