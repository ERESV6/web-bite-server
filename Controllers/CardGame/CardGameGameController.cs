using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Services.CardGame;

namespace web_bite_server.Controllers.CardGame
{
    [Route("api/card-game")]
    [ApiController]
    [Authorize]
    public class CardGameGameController : ControllerBase
    {
        private readonly IHubContext<CardGameHub, ICardGameHub> _hubContext;
        private readonly CardGameConnectionService _cardGameConnectionService;
        public CardGameGameController(IHubContext<CardGameHub, ICardGameHub> hubContext,

         CardGameConnectionService cardGameConnectionService)
        {
            _hubContext = hubContext;
            _cardGameConnectionService = cardGameConnectionService;
        }

        [HttpPost("play-card")]
        [Produces("application/json")]
        public async Task<ActionResult<int>> PlayCard([FromBody] int playedCardsNumber)
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);

            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyConnectionId).PlayCard(playedCardsNumber);
            return Ok(playedCardsNumber);
        }

        [HttpPost("end-turn")]
        [Produces("application/json")]
        public async Task<IActionResult> EndTurn()
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyConnectionId).EndTurn();
            return NoContent();
        }
    }
}