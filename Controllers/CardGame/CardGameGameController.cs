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
            var playedCards = await _cardGameGameService.CheckPlayedCards(cardGameIds, connectedUserCardGameConnection);

            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyUserConnection.ConnectionId).PlayCard(playedCards.Count);
            return Ok(playedCards.Count);
        }

        [HttpPost("end-turn")]
        [Produces("application/json")]
        public async Task<IActionResult> EndTurn([FromBody] List<int> cardGameIds)
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            var playedCards = await _cardGameGameService.CheckPlayedCards(cardGameIds, connectedUserCardGameConnection);

            await _cardGameGameService.EndTurn(playedCards, connectedUserCardGameConnection);

            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyUserConnection.ConnectionId).EndTurn();
            return NoContent();
        }

        [HttpPost("calculate-round-result")]
        [Produces("application/json")]
        public ActionResult<string> CalculateRoundResult()
        {
            // dodać rundy
            // wyliczyć wynik i wyświetlić
            // zabrać hp
            // trigger nowych 5 kart, sprawdzić max 10 kart na reke
            // i ogarnac te warunki z check card number bo juz nie bedzie na sztywno 5
            return Ok("ROUND ENDED");
        }
    }
}