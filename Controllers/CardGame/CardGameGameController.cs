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
        public async Task<ActionResult<List<CardGameCardDto>>> AddCardsToGame([FromBody] List<CardGameCardDto> cardGameCards)
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            var cardGameHand = await _cardGameGameService.AddCardsToCardGameHand(cardGameCards, connectedUserCardGameConnection);
            return Ok(cardGameHand);
        }

        [HttpPost("play-card")]
        [Produces("application/json")]
        public async Task<ActionResult<int>> PlayCard([FromBody] int playedCardsNumber)
        {
            // sprawdzić czy zagrane karty istnieją w ręce gracza
            // weryfikować czy zgadzają się rundy
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);

            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyUserConnection.ConnectionId).PlayCard(playedCardsNumber);
            return Ok(playedCardsNumber);
        }

        [HttpPost("end-turn")]
        [Produces("application/json")]
        public async Task<IActionResult> EndTurn()
        {
            // weryfikować czy zgadzają się rundy
            // sprawdzić czy zagrane karty istnieją w ręce gracza
            // po zakończeniu tury, dodać do bazy (podłączonego gracza) zagrane karty
            // na nowym endpoincie dodać pobranie kart moich i oponenta i na podstawie tego wyliczenia itp

            // system ulubionych kart - wyświetlają się jako pierwsze i podświetlone przed startem gry
            // system zawsze wyboru kart przed rozpoczęciem gry
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyUserConnection.ConnectionId).EndTurn();
            return NoContent();
        }
    }
}