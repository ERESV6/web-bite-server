using web_bite_server.Dtos.CardGame;
using web_bite_server.Repository;

namespace web_bite_server.Services.CardGame
{
    public class CardGameCardService
    {

        private readonly CardGameCardRepository _cardGameCardRepository;
        public CardGameCardService(CardGameCardRepository cardGameCardRepository)
        {
            _cardGameCardRepository = cardGameCardRepository;
        }

        public async Task<List<CardGameCardDto>> GetAllCards()
        {
            return await _cardGameCardRepository.GetAllCards();
        }
    }
}