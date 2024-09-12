using Microsoft.AspNetCore.SignalR;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Exceptions;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Models;
using web_bite_server.Repository;

namespace web_bite_server.Services.CardGame
{
    public class CardGameGameService
    {
        private readonly CardGameGameRepository _cardGameGameRepository;
        private readonly IHubContext<CardGameHub, ICardGameHub> _hubContext;
        public CardGameGameService(
            CardGameGameRepository cardGameGameRepository,
            IHubContext<CardGameHub, ICardGameHub> hubContext
        )
        {
            _cardGameGameRepository = cardGameGameRepository;
            _hubContext = hubContext;
        }

        public async Task<List<CardGameCardDto>?> AddCardsToCardGameHand(List<CardGameCardDto> cardGameCardsDto, CardGameUsersConnectionDto? cardGameUsersConnectionDto)
        {
            if (cardGameUsersConnectionDto?.UserConnection == null)
            {
                throw new NotFoundException("CONNECTED USER NOT FOUND");
            }

            var cardGameCards = cardGameCardsDto.Select(c => new CardGameCard
            {
                AttackValue = c.AttackValue,
                CardName = c.CardName,
                DefenseValue = c.DefenseValue,
                Id = c.Id,
                Label = c.Label,
                SpecialAbility = c.SpecialAbility
            });

            await _cardGameGameRepository.AddCardsToCardGameHand(cardGameUsersConnectionDto.UserConnection, cardGameCards);
            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHand(cardGameUsersConnectionDto.UserConnection);

            await _hubContext.Clients.Client(cardGameUsersConnectionDto.EnemyUserConnection.ConnectionId).SendRound(1);

            return cardGameHand;
        }
    }
}