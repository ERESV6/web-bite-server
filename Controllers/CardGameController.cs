using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;

namespace web_bite_server.Controllers
{
    [Route("api/card-game")]
    [ApiController]
    [Authorize]
    public class CardGameController : ControllerBase
    {
        private readonly IHubContext<UsersHub, IUsersHub> _hubContext;
        private readonly ApplicationDBContext _dBContext;
        public CardGameController(IHubContext<UsersHub, IUsersHub> hubContex, ApplicationDBContext dBContext)
        {
            _hubContext = hubContex;
            _dBContext = dBContext;
        }

        [HttpGet("request-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> RequestGameConnection([FromQuery] string userConnectionId, [FromQuery] string userToConnectionId )
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
                var updatedConnections =  await _dBContext.GameConnection.ToListAsync();
                var activeUsers = updatedConnections.Select(c => new CardGameActiveUserDto {
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
        public async Task CancelGameConnection( [FromQuery] string userToConnectionId, [FromQuery] string userConnectionId)
        {                        
            await _hubContext.Clients.Client(userToConnectionId).CancelGameConnection(userConnectionId);
        }

        [HttpGet("accept-connection")]
        public async Task AcceptGameConnection( [FromQuery] string userToConnectionId, [FromQuery] string userConnectionId)
        {                
            await _hubContext.Clients.Client(userToConnectionId).AcceptGameConnection(userConnectionId);
        }

        [HttpGet("decline-connection")]
        public async Task DeclineGameConnection( [FromQuery] string userToConnectionId, [FromQuery] string userConnectionId)
        {                        
            await _hubContext.Clients.Client(userToConnectionId).DeclineGameConnection(userConnectionId);
        }
    }
}