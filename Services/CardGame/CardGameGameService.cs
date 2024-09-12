using web_bite_server.Dtos.CardGame;
using web_bite_server.Exceptions;
using web_bite_server.Models;
using web_bite_server.Repository;

namespace web_bite_server.Services.CardGame
{
    public class CardGameGameService
    {
        private readonly CardGameGameRepository _cardGameGameRepository;
        public CardGameGameService(
            CardGameGameRepository cardGameGameRepository
        )
        {
            _cardGameGameRepository = cardGameGameRepository;
        }

        public async Task<List<CardGameCardDto>?> AddCardsToCardGameHand(List<CardGameCardDto> cardGameCardsDto, CardGameConnection? userConnection)
        {
            if (userConnection == null)
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

            await _cardGameGameRepository.AddCardsToCardGameHand(userConnection, cardGameCards);

            return await _cardGameGameRepository.GetUserCardGameHand(userConnection);
        }
    }
}