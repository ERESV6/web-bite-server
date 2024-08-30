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

        [HttpPost("request-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<CardGameConnectionDto>> RequestGameConnection([FromBody] CardGameConnectionDto cardGameConnectionDto)
        {
            var userConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == cardGameConnectionDto.UserConnectionId);
            var userToConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == cardGameConnectionDto.UserToConnectionId);

            if (
                userConnection != null && string.IsNullOrEmpty(userConnection.UserToId) && string.IsNullOrEmpty(userConnection.UserToRequestPendingId) &&
                userToConnection != null && string.IsNullOrEmpty(userToConnection.UserToId) && string.IsNullOrEmpty(userToConnection.UserToRequestPendingId)
                )
            {
                userConnection.UserToRequestPendingId = userToConnection.AppUserId;
                userToConnection.UserToRequestPendingId = userConnection.AppUserId;
                await _dBContext.SaveChangesAsync();
                await _hubContext.Clients.Client(cardGameConnectionDto.UserToConnectionId).RequestGameConnection(new CardGameConnectionDto
                {
                    UserConnectionId = cardGameConnectionDto.UserToConnectionId,
                    UserToConnectionId = cardGameConnectionDto.UserConnectionId
                });
                // do repository      
                var updatedConnections = await _dBContext.GameConnection.ToListAsync();
                var activeUsers = updatedConnections.Select(c => new CardGameActiveUserDto
                {
                    ConnectionId = c.ConnectionId,
                    UserName = c.AppUserName,
                    IsAvaliable = !(c.UserToId?.Length > 0 || c.UserToRequestPendingId?.Length > 0)
                }).ToList();
                await _hubContext.Clients.All.UserConnections(activeUsers);
                return Ok(cardGameConnectionDto);
            }
            return Ok(null);
        }

        [HttpPost("decline-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> DeclineGameConnection([FromBody] CardGameConnectionDto cardGameConnectionDto)
        {
            var userConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == cardGameConnectionDto.UserConnectionId);
            var userToConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == cardGameConnectionDto.UserToConnectionId);

            if (
                userConnection != null && string.IsNullOrEmpty(userConnection.UserToId) && !string.IsNullOrEmpty(userConnection.UserToRequestPendingId) &&
                userToConnection != null && string.IsNullOrEmpty(userToConnection.UserToId) && !string.IsNullOrEmpty(userToConnection.UserToRequestPendingId)
                )
            {
                userConnection.UserToRequestPendingId = string.Empty;
                userToConnection.UserToRequestPendingId = string.Empty;
                await _dBContext.SaveChangesAsync();
                await _hubContext.Clients.Client(cardGameConnectionDto.UserToConnectionId).DeclineGameConnection();
                // do repository      
                var updatedConnections = await _dBContext.GameConnection.ToListAsync();
                var activeUsers = updatedConnections.Select(c => new CardGameActiveUserDto
                {
                    ConnectionId = c.ConnectionId,
                    UserName = c.AppUserName,
                    IsAvaliable = !(c.UserToId?.Length > 0 || c.UserToRequestPendingId?.Length > 0)
                }).ToList();
                await _hubContext.Clients.All.UserConnections(activeUsers);
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost("accept-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> AcceptGameConnection()
        {
            var userAccept = await _userManager.GetUserAsync(HttpContext.User);
            if (userAccept == null)
            {
                return NotFound("Accepting user not found");
            }

            var userAcceptGameConnection = await _dBContext.GameConnection.FirstOrDefaultAsync(gc => gc.Id == userAccept.GameConnectionId);
            if (string.IsNullOrEmpty(userAcceptGameConnection?.UserToRequestPendingId))
            {
                return NotFound("Accepting user connection not found");
            }
            if (!string.IsNullOrEmpty(userAcceptGameConnection.UserToId))
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
            if (!string.IsNullOrEmpty(userReceiveGameConnection.UserToId))
            {
                return NotFound("Receiving User already connected with other user");
            }

            userAcceptGameConnection.UserToRequestPendingId = string.Empty;
            userReceiveGameConnection.UserToRequestPendingId = string.Empty;

            userAcceptGameConnection.UserToId = userReceive.Id;
            userReceiveGameConnection.UserToId = userAccept.Id;

            await _dBContext.SaveChangesAsync();
            await _hubContext.Clients.Client(userReceiveGameConnection.ConnectionId).AcceptGameConnection();
            return Ok(true);
        }

        [HttpGet("check-game-connection")]
        [Produces("application/json")]
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