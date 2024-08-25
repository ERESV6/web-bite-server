using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        public CardGameController(IHubContext<UsersHub, IUsersHub> hubContex)
        {
            _hubContext = hubContex;
        }

        [HttpGet("request-connection")]
        public async Task RequestGameConnection( [FromQuery] string userToConnectionId, [FromQuery] string userConnectionId)
        {                        
            await _hubContext.Clients.Client(userToConnectionId).RequestGameConnection(userConnectionId);
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