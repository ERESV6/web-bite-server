using web_bite_server.Dtos.CardGame;
using web_bite_server.Interfaces.CardGame;

namespace web_bite_server.Services.CardGame
{
    public class CardGameCardService
    {

        private readonly ICardGameCardRepository _cardGameCardRepository;
        public CardGameCardService(ICardGameCardRepository cardGameCardRepository)
        {
            _cardGameCardRepository = cardGameCardRepository;
        }

        public async Task<List<CardGameCardDto>> GetAllCards()
        {
            return await _cardGameCardRepository.GetAllCards();
        }
    }
}