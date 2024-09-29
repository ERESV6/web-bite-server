using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Mappers;

namespace web_bite_server.Repository
{
    public class CardGameCardRepository
    {
        private readonly ApplicationDBContext _dBContext;
        public CardGameCardRepository(ApplicationDBContext dBContext)
        {
            _dBContext = dBContext;
        }
        public async Task<List<CardGameCardDto>> GetAllCards()
        {
            return await _dBContext.CardGameCard.Select(c => c.ToCardGameCardDto()).ToListAsync();
        }

        public async Task<List<CardGameCardDto>> GetCardsByIds(List<int> cardGameIds)
        {
            return await _dBContext.CardGameCard.Where(c => cardGameIds.Contains(c.Id)).Select(c => c.ToCardGameCardDto()).ToListAsync();
        }

        public async Task<List<CardGameCardDto>> GetCardsExceptIds(IEnumerable<int> cardGameIds)
        {
            return await _dBContext.CardGameCard.Where(c => !cardGameIds.Contains(c.Id)).Select(c => c.ToCardGameCardDto()).ToListAsync();
        }

    }
}