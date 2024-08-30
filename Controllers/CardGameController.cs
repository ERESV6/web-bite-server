using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Controllers
{
    [Route("api/card-game")]
    [ApiController]
    [Authorize]
    public class CardGameController(IHubContext<UsersHub, IUsersHub> hubContex, ApplicationDBContext dBContext, UserManager<AppUser> userManager) : ControllerBase
    {
        private readonly IHubContext<UsersHub, IUsersHub> _hubContext = hubContex;
        private readonly ApplicationDBContext _dBContext = dBContext;
        private readonly UserManager<AppUser> _userManager = userManager;

        [HttpGet("request-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> RequestGameConnection([FromQuery] string userConnectionId, [FromQuery] string userToConnectionId)
        {
            var userConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == userConnectionId);
            var userToConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == userToConnectionId);

            if (
                userConnection != null && userConnection.UserToId == null && userConnection.UserToRequestPendingId == null &&
                userToConnection != null && userToConnection.UserToId == null && userToConnection.UserToRequestPendingId == null
                )
            {
                userConnection.UserToRequestPendingId = userToConnection.AppUserId;
                userToConnection.UserToRequestPendingId = userConnection.AppUserId;
                await _dBContext.SaveChangesAsync();
                await _hubContext.Clients.Client(userToConnectionId).RequestGameConnection(userConnectionId);
                // do repository      
                var updatedConnections = await _dBContext.GameConnection.ToListAsync();
                var activeUsers = updatedConnections.Select(c => new CardGameActiveUserDto
                {
                    ConnectionId = c.ConnectionId,
                    UserName = c.AppUserName,
                    IsAvaliable = !(c.UserToId?.Length > 0 || c.UserToRequestPendingId?.Length > 0)
                }).ToList();
                await _hubContext.Clients.All.UserConnection("połączenia elo do zmiany", activeUsers);
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpGet("cancel-connection")]
        public async Task CancelGameConnection([FromQuery] string userConnectionId, [FromQuery] string userToConnectionId)
        {
            await _hubContext.Clients.Client(userToConnectionId).CancelGameConnection(userConnectionId);
        }

        [HttpGet("accept-connection")]
        public async Task<ActionResult<bool>> AcceptGameConnection()
        {
            var userAccept = await _userManager.GetUserAsync(HttpContext.User);
            if (userAccept == null)
            {
                return NotFound("Accepting user not found");
            }

            var userAcceptGameConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.Id == userAccept.GameConnectionId);
            if (userAcceptGameConnection?.UserToRequestPendingId == null)
            {
                return NotFound("Accepting user connection not found");
            }
            if (userAcceptGameConnection.UserToId != null)
            {
                return NotFound("Accepting User already connected with other user");
            }

            var userReceive = await _userManager.FindByIdAsync(userAcceptGameConnection.UserToRequestPendingId);
            if (userReceive == null)
            {
                return NotFound("Game requesting user not found");
            }

            var userReceiveGameConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.Id == userReceive.GameConnectionId);
            if (userReceiveGameConnection == null)
            {
                return NotFound("Game requesting user connection not found");
            }
            if (userReceiveGameConnection.UserToId != null)
            {
                return NotFound("Receiving User already connected with other user");
            }

            userAcceptGameConnection.UserToRequestPendingId = string.Empty;
            userReceiveGameConnection.UserToRequestPendingId = string.Empty;

            userAcceptGameConnection.UserToId = userReceive.Id;
            userReceiveGameConnection.UserToId = userAccept.Id;

            await _dBContext.SaveChangesAsync();
            await _hubContext.Clients.Client(userReceiveGameConnection.ConnectionId).AcceptGameConnection(userAcceptGameConnection.ConnectionId);
            return Ok(true);
        }

        [HttpGet("decline-connection")]
        public async Task DeclineGameConnection([FromQuery] string userConnectionId, [FromQuery] string userToConnectionId)
        {
            await _hubContext.Clients.Client(userToConnectionId).DeclineGameConnection(userConnectionId);
        }

        [HttpGet("check-game-connection")]
        public async Task<ActionResult<bool>> CheckGameConnection()
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser == null)
            {
                return NotFound("User not found");
            }

            var userGameConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.Id == appUser.GameConnectionId);
            if (userGameConnection?.UserToId == null)
            {
                return NotFound("User not connected with other user");
            }
            return Ok(true);
        }
    }
}