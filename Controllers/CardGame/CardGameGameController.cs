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

        [HttpGet("all-except-hand")]
        [Produces("application/json")]
        public async Task<ActionResult<List<CardGameCardDto>>> GetAllCardsExceptPlayerHand()
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            return Ok(await _cardGameGameService.GetAllCardsExceptPlayerHand(connectedUserCardGameConnection.UserConnection));
        }

        [HttpGet("enemy-hand")]
        [Produces("application/json")]
        public async Task<ActionResult<int>> GetNumberOfEnemyPlayerHand()
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            return Ok(await _cardGameGameService.GetNumberOfEnemyPlayerHand(connectedUserCardGameConnection.EnemyUserConnection));
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
        public async Task<ActionResult<RoundResultDto>> CalculateRoundResult()
        {
            var connectedUserCardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);
            var roundResult = await _cardGameGameService.CalculateRoundResult(connectedUserCardGameConnection);
            await _hubContext.Clients.Client(connectedUserCardGameConnection.EnemyUserConnection.ConnectionId).CalculatedRoundResults(new RoundResultDto
            {
                PlayerHitpoints = roundResult.EnemyHitpoints,
                EnemyHitpoints = roundResult.PlayerHitpoints,
                Round = roundResult.Round,
                PlayerPoints = roundResult.EnemyPoints,
                EnemyPoints = roundResult.PlayerPoints,
                DamageDoneToEnemy = roundResult.DamageDoneToPlayer,
                DamageDoneToPlayer = roundResult.DamageDoneToEnemy,
                EnemyAttack = roundResult.PlayerAttack,
                EnemyDefense = roundResult.PlayerDefense,
                PlayerAttack = roundResult.EnemyAttack,
                PlayerDefense = roundResult.EnemyDefense,
                PlayerPlayedCards = roundResult.EnemyPlayedCards,
                EnemyPlayedCards = roundResult.PlayerPlayedCards,
                IsEndRound = roundResult.IsEndRound,
                IsPlayerWinner = roundResult.IsEnemyWinner,
                IsEnemyWinner = roundResult.IsPlayerWinner
            });
            return Ok(roundResult);
        }
    }
}