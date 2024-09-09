using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Interfaces.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Controllers.CardGame
{
    [Route("api/card-game")]
    [ApiController]
    [Authorize]
    public class CardGameGameController : ControllerBase
    {
        /*
            @TODO - POMYŚLEĆ O WYDZIELENIU:
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser == null || appUser?.CardGameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }
            var userConnection = await _cardGameRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);

            poczytać o rodzajach service
            pomyśleć nad tymi warunkami czy nie jest ich czasem za duzo
        */
        private readonly IHubContext<CardGameHub, ICardGameHub> _hubContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICardGameConnectionRepository _cardGameConnectionRepository;
        public CardGameGameController(IHubContext<CardGameHub, ICardGameHub> hubContext, UserManager<AppUser> userManager, ICardGameConnectionRepository cardGameConnectionRepository)
        {
            _hubContext = hubContext;
            _userManager = userManager;
            _cardGameConnectionRepository = cardGameConnectionRepository;
        }

        [HttpPost("play-card")]
        [Produces("application/json")]
        public async Task<ActionResult<int>> PlayCard([FromBody] int playedCardsNumber)
        {
            var appUser = await _userManager.GetUserAsync(HttpContext.User);
            if (appUser?.CardGameConnectionId == null)
            {
                return NotFound("User not found or is not connected to the game");
            }

            var userCardGameConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appUser.CardGameConnectionId);
            if (userCardGameConnection?.UserToId == null)
            {
                return NotFound("User not connected with other user");
            }

            var appConnectedUser = await _userManager.FindByIdAsync(userCardGameConnection.UserToId);
            if (appConnectedUser?.CardGameConnectionId == null)
            {
                return NotFound("Connected User not found or is not connected to the game");
            }

            var connectedUserCardGameConnection = await _cardGameConnectionRepository.GetCardGameConnectionByCardGameConnectionId(appConnectedUser.CardGameConnectionId);
            if (connectedUserCardGameConnection?.UserToId == null)
            {
                return NotFound("Connected User not connected with other user");
            }

            if (userCardGameConnection.UserToId != appConnectedUser.Id || connectedUserCardGameConnection.UserToId != appUser.Id)
            {
                return NotFound("User and connected User ids dont match");
            }

            await _hubContext.Clients.Client(connectedUserCardGameConnection.ConnectionId).PlayCard(playedCardsNumber);
            return Ok(playedCardsNumber);
        }
    }
}