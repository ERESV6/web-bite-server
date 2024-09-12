
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Services.CardGame;

namespace web_bite_server.Controllers
{
    [Route("api/game-card")]
    [ApiController]
    [Authorize]
    public class CardGameCardController : ControllerBase
    {

        private readonly CardGameCardService _cardGameCardService;
        public CardGameCardController(CardGameCardService cardGameCardService)
        {
            _cardGameCardService = cardGameCardService;
        }

        [HttpGet("all")]
        [Produces("application/json")]
        public async Task<ActionResult<List<CardGameCardDto>>> GetAllCards()
        {
            return Ok(await _cardGameCardService.GetAllCards());
        }
    }
}