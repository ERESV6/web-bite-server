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
    [Route("api/card-game")]
    [ApiController]
    [Authorize]
    public class CardGameController(IHubContext<UsersHub, IUsersHub> hubContex, UserManager<AppUser> userManager, ICardGameRepository cardGameRepository) : ControllerBase
    {
        private readonly IHubContext<UsersHub, IUsersHub> _hubContext = hubContex;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ICardGameRepository _cardGameRepository = cardGameRepository;

        [HttpPost("request-connection/{userToConnectionId}")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> RequestGameConnection([FromRoute] string userToConnectionId)
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.GameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(appUser.GameConnectionId);
            var userToConnection = await _cardGameRepository.GetGameConnectionByConnectionId(userToConnectionId);

            if (
                userConnection != null && string.IsNullOrEmpty(userConnection.UserToId) && string.IsNullOrEmpty(userConnection.UserToRequestPendingId) &&
                userToConnection != null && string.IsNullOrEmpty(userToConnection.UserToId) && string.IsNullOrEmpty(userToConnection.UserToRequestPendingId)
                )
            {
                await _cardGameRepository.UpdateConnectionUsersPendingIds(userConnection, userToConnection);
                await _hubContext.Clients.Client(userToConnection.ConnectionId).RequestGameConnection();

                var allActiveGameConnections = await _cardGameRepository.GetAllActiveGameConnectionsAsync();
                await _hubContext.Clients.All.UserConnections(allActiveGameConnections);
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost("decline-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> DeclineGameConnection()
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.GameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(appUser.GameConnectionId);
            if (string.IsNullOrEmpty(userConnection?.UserToRequestPendingId))
            {
                return NotFound("User is not waiting for connection");
            }

            var appPendingUser = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (appPendingUser?.GameConnectionId == null)
            {
                return NotFound("Pending User not found or is not connected to the game");
            }

            var userToConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(appPendingUser.GameConnectionId);
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
                await _hubContext.Clients.Client(userToConnection.ConnectionId).DeclineGameConnection();

                var allActiveGameConnections = await _cardGameRepository.GetAllActiveGameConnectionsAsync();
                await _hubContext.Clients.All.UserConnections(allActiveGameConnections);
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost("accept-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> AcceptGameConnection()
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.GameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(appUser.GameConnectionId);
            if (string.IsNullOrEmpty(userConnection?.UserToRequestPendingId))
            {
                return NotFound("User is not waiting for connection");
            }
            if (!string.IsNullOrEmpty(userConnection.UserToId))
            {
                return NotFound("User is already connected with other user");
            }

            var userReceive = await _userManager.FindByIdAsync(userConnection.UserToRequestPendingId);
            if (userReceive?.GameConnectionId == null)
            {
                return NotFound("Game requesting user not found or is not connected to the game");
            }

            var userReceiveGameConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(userReceive.GameConnectionId);
            if (string.IsNullOrEmpty(userReceiveGameConnection?.UserToRequestPendingId))
            {
                return NotFound("Game requesting user not found or is not waiting for connection");
            }
            if (!string.IsNullOrEmpty(userReceiveGameConnection?.UserToId))
            {
                return NotFound("Game requesting user is already connected with other user");
            }

            if (userConnection != null && userReceiveGameConnection != null)
            {
                await _cardGameRepository.ConnectConnectionUsersIds(userConnection, userReceiveGameConnection);
                await _hubContext.Clients.Client(userReceiveGameConnection.ConnectionId).AcceptGameConnection();
                return Ok(true);
            }

            return Ok(false);
        }

        [HttpGet("check-game-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> CheckGameConnection()
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.GameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userGameConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(appUser.GameConnectionId);
            if (userGameConnection?.UserToId == null)
            {
                return NotFound("User not connected with other user");
            }

            var appConnectedUser = await _userManager.FindByIdAsync(userGameConnection.UserToId);
            if (appConnectedUser?.GameConnectionId == null)
            {
                return NotFound("Connected User not found or is not connected to the game");
            }

            var connectedUserGameConnection = await _cardGameRepository.GetGameConnectionByGameConnectionId(appConnectedUser.GameConnectionId);
            if (connectedUserGameConnection?.UserToId == null)
            {
                return NotFound("Connected User not connected with other user");
            }
            if (userGameConnection.UserToId == appConnectedUser.Id && connectedUserGameConnection.UserToId == appUser.Id)
            {
                return Ok(true);
            }
            return NotFound("User and connected User ids dont match");
        }
    }
}