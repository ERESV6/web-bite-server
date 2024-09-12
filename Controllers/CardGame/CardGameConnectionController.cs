using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Services.CardGame;

namespace web_bite_server.Controllers
{
    [Route("api/card-game-connection")]
    [ApiController]
    [Authorize]
    public class CardGameConnectionController(CardGameConnectionService cardGameConnectionService) : ControllerBase
    {
        private readonly CardGameConnectionService _cardGameConnectionService = cardGameConnectionService;

        [HttpPost("request-connection/{userToConnectionId}")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> RequestCardGameConnection([FromRoute] string userToConnectionId)
        {
            return Ok(await _cardGameConnectionService.RequestCardGameConnection(userToConnectionId, HttpContext.User));
        }

        [HttpPost("decline-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> DeclineCardGameConnection()
        {
            return Ok(await _cardGameConnectionService.DeclineCardGameConnection(HttpContext.User));
        }

        [HttpPost("accept-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> AcceptCardGameConnection()
        {
            return Ok(await _cardGameConnectionService.AcceptCardGameConnection(HttpContext.User));
        }

        [HttpGet("check-game-connection")]
        [Produces("application/json")]
        public async Task<ActionResult<CardGameConnectionDto>> CheckCardGameConnection()
        {
            var cardGameConnection = await _cardGameConnectionService.CheckCardGameConnection(HttpContext.User);

            return Ok(new CardGameConnectionDto
            {
                UserName = cardGameConnection.UserConnection.AppUser?.UserName ?? "",
                EnemyUserName = cardGameConnection.EnemyUserConnection.AppUser?.UserName ?? ""
            });
        }
    }
}