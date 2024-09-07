using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Interfaces.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Controllers
{
    [Route("api/card-game-connection")]
    [ApiController]
    [Authorize]
    public class CardGameConnectionController(IHubContext<CardGameHub, ICardGameHub> hubContex, UserManager<AppUser> userManager, ICardGameConnectionRepository cardGameRepository) : ControllerBase
    {
        private readonly IHubContext<CardGameHub, ICardGameHub> _hubContext = hubContex;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ICardGameConnectionRepository _cardGameRepository = cardGameRepository;

        [HttpPost("request-connection/{userToConnectionId}")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> RequestCardGameConnection([FromRoute] string userToConnectionId)
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser == null || appUser?.CardGameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
            var userToConnection = await _cardGameRepository.GetCardGameConnectionByConnectionId(userToConnectionId);

            if (
                userConnection != null && string.IsNullOrEmpty(userConnection.UserToId) && string.IsNullOrEmpty(userConnection.UserToRequestPendingId) &&
                userToConnection != null && string.IsNullOrEmpty(userToConnection.UserToId) && string.IsNullOrEmpty(userToConnection.UserToRequestPendingId)
                )
            {
                await _cardGameRepository.UpdateConnectionUsersPendingIds(userConnection, userToConnection);
                await _hubContext.Clients.Client(userToConnection.ConnectionId).RequestCardGameConnection(appUser.UserName ?? "");

                var allActiveCardGameConnections = await _cardGameRepository.GetAllActiveCardGameConnectionsAsync();
                await _hubContext.Clients.All.UserConnections(allActiveCardGameConnections);
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost("decline-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> DeclineCardGameConnection()
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.CardGameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
            if (string.IsNullOrEmpty(userConnection?.UserToRequestPendingId))
            {
                return NotFound("User is not waiting for connection");
            }

            var appPendingUser = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (appPendingUser?.CardGameConnectionId == null)
            {
                return NotFound("Pending User not found or is not connected to the game");
            }

            var userToConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appPendingUser.CardGameConnectionId);
            if (string.IsNullOrEmpty(userToConnection?.UserToRequestPendingId))
            {
                return NotFound("Pending User is not waiting for connection");
            }

            if (
                userConnection != null && string.IsNullOrEmpty(userConnection.UserToId) &&
                userToConnection != null && string.IsNullOrEmpty(userToConnection.UserToId)
                )
            {
                await _cardGameRepository.CleanConnectionUsersPendingIds(userConnection, userToConnection);
                await _hubContext.Clients.Client(userToConnection.ConnectionId).DeclineCardGameConnection();

                var allActiveCardGameConnections = await _cardGameRepository.GetAllActiveCardGameConnectionsAsync();
                await _hubContext.Clients.All.UserConnections(allActiveCardGameConnections);
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost("accept-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> AcceptCardGameConnection()
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.CardGameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
            if (string.IsNullOrEmpty(userConnection?.UserToRequestPendingId))
            {
                return NotFound("User is not waiting for connection");
            }
            if (!string.IsNullOrEmpty(userConnection.UserToId))
            {
                return NotFound("User is already connected with other user");
            }

            var userReceive = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (userReceive?.CardGameConnectionId == null)
            {
                return NotFound("Game requesting user not found or is not connected to the game");
            }

            var userReceiveCardGameConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(userReceive.CardGameConnectionId);
            if (string.IsNullOrEmpty(userReceiveCardGameConnection?.UserToRequestPendingId))
            {
                return NotFound("Game requesting user not found or is not waiting for connection");
            }
            if (!string.IsNullOrEmpty(userReceiveCardGameConnection?.UserToId))
            {
                return NotFound("Game requesting user is already connected with other user");
            }

            if (userConnection != null && userReceiveCardGameConnection != null)
            {
                await _cardGameRepository.ConnectConnectionUsersIds(userConnection, userReceiveCardGameConnection);
                await _hubContext.Clients.Client(userReceiveCardGameConnection.ConnectionId).AcceptCardGameConnection();
                return Ok(true);
            }

            return Ok(false);
        }

        [HttpGet("check-game-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> CheckCardGameConnection()
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.CardGameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userCardGameConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
            if (userCardGameConnection?.UserToId == null)
            {
                return NotFound("User not connected with other user");
            }

            var appConnectedUser = await _userManager.FindByIdAsync(userCardGameConnection.UserToId);
            if (appConnectedUser?.CardGameConnectionId == null)
            {
                return NotFound("Connected User not found or is not connected to the game");
            }

            var connectedUserCardGameConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appConnectedUser.CardGameConnectionId);
            if (connectedUserCardGameConnection?.UserToId == null)
            {
                return NotFound("Connected User not connected with other user");
            }
            if (userCardGameConnection.UserToId == appConnectedUser.Id && connectedUserCardGameConnection.UserToId == appUser.Id)
            {
                return Ok(true);
            }
            return NotFound("User and connected User ids dont match");
        }
    }
}