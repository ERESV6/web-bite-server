using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.Dtos.CardGame;
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
        private readonly CardGameGameService _cardGameGameService;
        public CardGameGameController(
            IHubContext<CardGameHub, ICardGameHub> hubContext,
            CardGameConnectionService cardGameConnectionService,
            CardGameGameService cardGameGameService
        )
        {
            _hubContext = hubContext;
            _cardGameConnectionService = cardGameConnectionService;
            _cardGameGameService = cardGameGameService;
        }

        [HttpPost("add-cards-to-game")]
        [Produces("application/json")]
        public async Task<ActionResult<List<CardGameCardDto>>> AddCardsToGame([FromBody] List<int> cardGameIds)
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            var cardGameHand = await _cardGameGameService.AddCardsToCardGameHand(cardGameIds, connectedUserCardGameConnection);
            return Ok(cardGameHand);
        }

        [HttpPost("play-card")]
        [Produces("application/json")]
        public async Task<ActionResult<int>> PlayCard([FromBody] List<int> cardGameIds)
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            var playedCardsNumber = await _cardGameGameService.CheckPlayedCards(cardGameIds, connectedUserCardGameConnection);

            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyUserConnection.ConnectionId).PlayCard(playedCardsNumber);
            return Ok(playedCardsNumber);
        }

        [HttpPost("end-turn")]
        [Produces("application/json")]
        public async Task<IActionResult> EndTurn([FromBody] List<int> cardGameIds)
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            await _cardGameGameService.CheckPlayedCards(cardGameIds, connectedUserCardGameConnection);

            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyUserConnection.ConnectionId).EndTurn();
            return NoContent();
        }
    }
}