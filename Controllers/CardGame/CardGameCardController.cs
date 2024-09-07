
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Interfaces.CardGame;

namespace web_bite_server.Controllers
{
    [Route("api/game-card")]
    [ApiController]
    [Authorize]
    public class CardGameCardController : ControllerBase
    {
        private readonly ICardGameCardRepository _cardGameCardRepository;
        public CardGameCardController(ICardGameCardRepository cardGameCardRepository)
        {
            _cardGameCardRepository = cardGameCardRepository;
        }

        [HttpGet("all")]
        [Produces("application/json")]
        public async Task<ActionResult<List<CardGameCardDto>>> GetAllCards()
        {
            var cards = await _cardGameCardRepository.GetAllCards();
            return Ok(cards);
        }

    }
}